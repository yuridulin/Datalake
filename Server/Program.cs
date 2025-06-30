using Datalake.Database;
using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Repositories;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.Server.Middlewares;
using Datalake.Server.Services;
using LinqToDB;
using LinqToDB.AspNet;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Generation;
using Serilog;
using Serilog.Events;
using System.Reflection;

[assembly: AssemblyVersion("2.3.*")]

namespace Datalake.Server
{
	/// <summary>
	/// Основной класс приложения
	/// </summary>
	public class Program
	{
		internal static string WebRootPath { get; set; } = string.Empty;

		/// <summary>
		/// Метод запуска приложения
		/// </summary>
		/// <param name="args">Аргументы, с которыми оно запускается</param>
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			var storage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage");
			builder.Configuration
				.SetBasePath(storage)
				.AddJsonFile(Path.Combine(Path.Combine(storage, "config"), "appsettings.json"), false, true)
				.AddJsonFile(Path.Combine(Path.Combine(storage, "config"), $"appsettings.{builder.Environment.EnvironmentName}.json"), true,
					true);
			Directory.CreateDirectory(Path.Combine(storage, "logs"));

			builder.Services.AddControllers().AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
			});
			builder.Services.AddOpenApiDocument((options, services) =>
			{
				options.DocumentName = "Datalake App";
				options.Title = "Datalake App";
				options.Version = "v" + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
				options.SchemaSettings.GenerateEnumMappingDescription = true;
				options.SchemaSettings.UseXmlDocumentation = true;
				options.SchemaSettings.SchemaProcessors.Add(new XEnumVarnamesNswagSchemaProcessor());
			});
			builder.Services.AddEndpointsApiExplorer();

			builder.AddMiddlewares();
			ConfigureDatabase(builder);
			builder.AddServices();

			Log.Logger = new LoggerConfiguration()
				.ReadFrom.Configuration(builder.Configuration)
				.CreateLogger();

			builder.Host.UseSerilog();

			var app = builder.Build();

			WebRootPath = app.Environment.WebRootPath;
			StartWorkWithDatabase(app);

			if (app.Environment.IsDevelopment())
			{
				app.UseOpenApi();
				app.UseSwaggerUi();
			}

			app.UseSerilogRequestLogging(options =>
			{
				// шаблон одного сообщения на запрос
				options.MessageTemplate = "HTTP: [{Controller}.{Action}] > {StatusCode} in {Elapsed:0.0000} ms";

				// если упало — логируем Error, иначе Information
				options.GetLevel = (httpContext, elapsed, ex) =>
				httpContext.Request.Method == "OPTIONS"
						? LogEventLevel.Verbose // или LogEventLevel.None, если используешь фильтрацию
						: ex != null || httpContext.Response.StatusCode >= 500
							? LogEventLevel.Error
							: LogEventLevel.Information;

				options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
				{
					var endpoint = httpContext.GetEndpoint();
					var routePattern = endpoint?.Metadata.GetMetadata<RouteNameMetadata>();
					var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

					if (actionDescriptor != null)
					{
						diagnosticContext.Set("Controller", actionDescriptor.ControllerName);
						diagnosticContext.Set("Action", actionDescriptor.ActionName);
					}
					else
					{
						diagnosticContext.Set("Controller", "unknown");
						diagnosticContext.Set("Action", "unknown");
					}
				};
			});

			app.UseDefaultFiles();
			app.UseStaticFiles();
			app.UseHttpsRedirection();
			app.UseRouting();
			app.UseCors(policy =>
			{
				policy
					.AllowAnyMethod()
					.AllowAnyOrigin()
					.AllowAnyHeader()
					.WithExposedHeaders([
						AuthConstants.TokenHeader,
						AuthConstants.GlobalAccessHeader,
						AuthConstants.NameHeader,
						AuthConstants.UnderlyingUserGuidHeader,
					]);
			});
			app.UseMiddlewares();

			app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}

		static void ConfigureDatabase(WebApplicationBuilder builder)
		{
			var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";
			var expectedVariables = connectionString.Split("${")
				.Select(part =>
				{
					int endSymbol = part.IndexOf('}');
					if (part.Contains('}'))
						return part[..endSymbol];
					else
						return null;
				})
				.Where(x => x != null)
				.ToArray();

			var env = Environment.GetEnvironmentVariables();
			foreach (var variable in expectedVariables)
			{
				var value = env.Contains(variable!) ? env[variable!]?.ToString() : null;
				if (string.IsNullOrEmpty(value))
					throw new Exception("Expected ENV variable is not found: " + variable);
				else
					connectionString = connectionString.Replace($"${{{variable}}}", value);
			}

			builder.Services.AddDbContext<DatalakeEfContext>(options =>
				options
					.UseNpgsql(connectionString, config => config.CommandTimeout(300))
			);

			builder.Services.AddLinqToDBContext<DatalakeContext>((provider, options) =>
				options
					.UsePostgreSQL(connectionString ?? throw new Exception("Connection string not provided"))
			);
		}

		static async void StartWorkWithDatabase(WebApplication app)
		{
			using var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope()
				?? throw new Exception("Серьезно?");

			var ef = serviceScope.ServiceProvider.GetRequiredService<DatalakeEfContext>();
			ef.Database.Migrate();

			DatalakeContext.LoggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
			DatalakeContext.SetupLinqToDB();

			var db = serviceScope.ServiceProvider.GetRequiredService<DatalakeContext>();
			var dataStore = serviceScope.ServiceProvider.GetRequiredService<DatalakeDataStore>();
			var usersRepository = serviceScope.ServiceProvider.GetRequiredService<UsersMemoryRepository>();

			await db.EnsureDataCreatedAsync(dataStore, usersRepository);
			await AuditRepository.WriteAsync(
				db,
				"Сервер запущен",
				category: LogCategory.Core,
				type: LogType.Success
			);
		}

		internal class XEnumVarnamesNswagSchemaProcessor : ISchemaProcessor
		{
			public void Process(SchemaProcessorContext context)
			{
				if (context.ContextualType.OriginalType.IsEnum)
				{
					if (context.Schema.ExtensionData is not null)
					{
						context.Schema.ExtensionData.Add("x-enum-varnames", context.Schema.EnumerationNames.ToArray());
					}
					else
					{
						context.Schema.ExtensionData = new Dictionary<string, object?>()
						{
								{"x-enum-varnames", context.Schema.EnumerationNames.ToArray()}
						};
					}
				}
			}
		}
	}
}
