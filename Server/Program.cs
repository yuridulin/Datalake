using Datalake.ApiClasses.Exceptions.Base;
using Datalake.Database;
using Datalake.Database.Repositories;
using Datalake.Server.BackgroundServices.Collector;
using Datalake.Server.BackgroundServices.Collector.Collectors.Factory;
using Datalake.Server.Constants;
using Datalake.Server.Middlewares;
using Datalake.Server.Services.Receiver;
using Datalake.Server.Services.SessionManager;
using LinqToDB;
using LinqToDB.AspNet;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Generation;
using Serilog;
using System.Reflection;
using Datalake.ApiClasses.Models.Settings;

#if DEBUG
using LinqToDB.AspNet.Logging;
#endif

namespace Datalake.Server
{
	/// <summary>
	/// Основной класс приложения
	/// </summary>
	public class Program
	{
		static string WebRootPath { get; set; } = string.Empty;

		/// <summary>
		/// Метод запуска приложения
		/// </summary>
		/// <param name="args">Аргументы, с которыми оно запускается</param>
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			builder.Services.AddControllers();
			builder.Services.AddOpenApiDocument((options, services) =>
			{
				options.DocumentName = "Datalake App";
				options.Title = "Datalake App";
				options.Version = "v" + Assembly.GetExecutingAssembly().GetName().Version?.ToString();
				options.SchemaSettings.GenerateEnumMappingDescription = true;
				options.SchemaSettings.UseXmlDocumentation = true;
				options.SchemaSettings.SchemaProcessors.Add(new XEnumVarnamesNswagSchemaProcessor());
			});

			builder.Host.UseSerilog((context, config) => config.ReadFrom.Configuration(context.Configuration));
			builder.Services.AddLogging(options => options.AddSerilog());
			builder.Services.AddEndpointsApiExplorer();

			ConfigureDatabase(builder);
			ConfigureServices(builder);

			var app = builder.Build();

			app.UseSerilogRequestLogging();

			WebRootPath = app.Environment.WebRootPath;
			StartWorkWithDatabase(app);

			if (app.Environment.IsDevelopment())
			{
				app.UseOpenApi();
				app.UseSwaggerUi();
			}

			app.UseDefaultFiles();
			app.UseStaticFiles();
			app.UseRouting();
			app.UseCors(policy =>
			{
				policy
					.AllowAnyMethod()
					.AllowAnyOrigin()
					.AllowAnyHeader()
					.WithExposedHeaders([
						AuthConstants.TokenHeader,
					]);
			});

			app.UseMiddleware<AuthMiddleware>();

			ConfigureErrorPage(app);

			app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");
			app.MapControllerRoute(
				name: "default",
				pattern: "{controller=Home}/{action=Index}/{id?}");

			app.Run();
		}

		static void ConfigureDatabase(WebApplicationBuilder builder)
		{
			var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";

			var env = Environment.GetEnvironmentVariables();
			foreach (var key in env.Keys)
			{
				var arg = "${" + key + "}";
				if (connectionString.Contains(arg))
				{
					connectionString = connectionString.Replace(arg, env[key]?.ToString() ?? arg);
				}
			}

			builder.Services.AddDbContext<DatalakeEfContext>(options =>
			{
				options
					.UseNpgsql(connectionString)
#if DEBUG
					.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug()))
#endif
					;
			});

			builder.Services.AddLinqToDBContext<DatalakeContext>((provider, options) =>
				options
					.UsePostgreSQL(connectionString ?? throw new Exception("�� �������� ������ ����������� � ���� ������"))
#if DEBUG
					.UseDefaultLogging(provider)
#endif
			);
		}

		static void ConfigureServices(WebApplicationBuilder builder)
		{
			// постоянные
			builder.Services.AddSingleton<CollectorFactory>();
			builder.Services.AddSingleton<ReceiverService>();
			builder.Services.AddSingleton<SessionManagerService>();

			// временные
			builder.Services.AddTransient<BlocksRepository>();
			builder.Services.AddTransient<TagsRepository>();
			builder.Services.AddTransient<SourcesRepository>();
			builder.Services.AddTransient<SystemRepository>();
			builder.Services.AddTransient<UsersRepository>();
			builder.Services.AddTransient<UserGroupsRepository>();
			builder.Services.AddTransient<ValuesRepository>();
			builder.Services.AddTransient<AuthMiddleware>();

			// службы
			builder.Services.AddHostedService<CollectorService>();
		}

		static async void StartWorkWithDatabase(WebApplication app)
		{
			using var serviceScope = app.Services?.GetService<IServiceScopeFactory>()?.CreateScope();

			var context = serviceScope?.ServiceProvider.GetRequiredService<DatalakeEfContext>();
			context?.Database.Migrate();

			DatalakeContext.SetupLinqToDB();
			var db = serviceScope?.ServiceProvider.GetRequiredService<DatalakeContext>();
			if (db != null)
			{
				await db.EnsureDataCreatedAsync();
				WriteStartipFile(await new SystemRepository(db).GetSettingsAsync());
			}
		}

		static void ConfigureErrorPage(WebApplication app)
		{
			app.UseExceptionHandler(exceptionHandlerApp =>
			{
				exceptionHandlerApp.Run(async context =>
				{
					var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

					var error = exceptionHandlerPathFeature?.Error;
					string message;

					if (error is DatalakeException)
					{
						message = error.ToString();
					}
					else
					{
						message = "Ошибка выполнения на сервере" +
							"\n\n" + // разделитель, по которому клиент отсекает служебную часть сообщения
							error?.ToString() ?? "error is null";
					}

					context.Response.StatusCode = StatusCodes.Status500InternalServerError;
					context.Response.ContentType = "text/plain; charset=UTF-8";

					await context.Response.WriteAsync(message);
				});
			});
		}

		/// <summary>
		/// Запись настроек, необходимых при запуске клиента, в клиентский файл
		/// </summary>
		/// <param name="settings">Текущие настройки</param>
		internal static void WriteStartipFile(SettingsInfo settings)
		{
			File.WriteAllLines(Path.Combine(WebRootPath, "startup.js"), [
				"var LOCAL_API = true;",
				$"var KEYCLOAK_DB = '{settings.EnergoIdHost}';",
			]);
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
