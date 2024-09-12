using Datalake.ApiClasses.Exceptions.Base;
using Datalake.Database;
using Datalake.Database.Extensions;
using Datalake.Database.Repositories;
using Datalake.Server.BackgroundServices.Collector;
using Datalake.Server.BackgroundServices.SettingsHandler;
using Datalake.Server.Constants;
using Datalake.Server.Middlewares;
using Datalake.Server.Services.Receiver;
using Datalake.Server.Services.SessionManager;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Generation;
using Serilog;
using System.Reflection;

[assembly: AssemblyVersion("2.0.*")]

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

			builder.Services.AddLogging(/*options => options.AddSerilog()*/);
			builder.Services.AddEndpointsApiExplorer();

			ConfigureDatabase(builder);
			ConfigureServices(builder);

			var app = builder.Build();

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

		/// <summary>
		/// Фабрика логгеров с основными настройками
		/// </summary>
		public static readonly ILoggerFactory MainLoggerFactory = LoggerFactory.Create(builder =>
		{
			builder
#if DEBUG
				.AddDebug()
#elif RELEASE
					.AddFilter("LinqToDB.Data.DataConnection", LogLevel.Warning)
					.AddFilter("LinqToDB.Data.DataConnection", LogLevel.Warning)
#endif
				.AddConsole();
		});

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
					.UseNpgsql(connectionString, config => config.CommandTimeout(300))
					.UseLoggerFactory(MainLoggerFactory);
			});

			builder.Services.AddLinqToDBContext<DatalakeContext>((provider, options) =>
				options
					.UsePostgreSQL(connectionString ?? throw new Exception("Connection string not provided"))
					.UseLoggerFactory(MainLoggerFactory)
			);

			AppContext.SetSwitch("Npgsql.EnableDiagnostics", true);
		}

		static void ConfigureServices(WebApplicationBuilder builder)
		{
			// постоянные
			builder.Services.AddSingleton<CollectorFactory>();
			builder.Services.AddSingleton<ReceiverService>();
			builder.Services.AddSingleton<SessionManagerService>();
			builder.Services.AddSingleton<SettingsHandlerService>();
			builder.Services.AddSingleton<ISettingsUpdater>(provider
				=> provider.GetRequiredService<SettingsHandlerService>());

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
			builder.Services.AddHostedService<CollectorProcessor>();
			builder.Services.AddHostedService<CollectorWriter>();
			builder.Services.AddHostedService<SettingsHandlerService>();
			builder.Services.AddHostedService(provider
				=> provider.GetRequiredService<SettingsHandlerService>());
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
				await db.LogAsync(new Database.Models.Log
				{
					Category = ApiClasses.Enums.LogCategory.Core,
					Type = ApiClasses.Enums.LogType.Success,
					Text = "Сервер запущен",
				});

				db.SetLastUpdateToNow();
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
