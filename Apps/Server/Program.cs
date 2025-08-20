using Datalake.Database;
using Datalake.Database.Functions;
using Datalake.Database.Initialization;
using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Repositories;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Models.Tags;
using Datalake.Server.Middlewares;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.Collection;
using Datalake.Server.Services.Maintenance;
using Datalake.Server.Services.Receiver;
using Datalake.Server.Services.SettingsHandler;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using LinqToDB.Mapping;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NJsonSchema.Generation;
using Npgsql;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Datalake.Server;

/// <summary>
/// Datalake Server
/// </summary>
public class Program
{
	internal static string WebRootPath { get; set; } = string.Empty;
	internal static string CurrentEnvironment { get; set; } = string.Empty;
	internal static string Version { get; set; } =
		Environment.GetEnvironmentVariable("APP_VERSION")
			?? Assembly.GetExecutingAssembly().GetName().Version?.ToString()
			?? "";

	/// <summary>
	/// Старт Datalake Server
	/// </summary>
	public static async Task Main(string[] args)
	{
		// дефолт сообщение, чтобы увидеть факт запуска
		Console.WriteLine($"Запуск Datalake: v{Version}");

		// настройка
		var builder = WebApplication.CreateBuilder(args);
		CurrentEnvironment = builder.Environment.EnvironmentName;

		// конфигурация
		var storage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage");
		builder.Configuration
			.SetBasePath(storage)
			.AddJsonFile(
				path: Path.Combine(Path.Combine(storage, "config"), "appsettings.json"),
				optional: false,
				reloadOnChange: true)
			.AddJsonFile(
				path: Path.Combine(Path.Combine(storage, "config"), $"appsettings.{CurrentEnvironment}.json"),
				optional: true,
				reloadOnChange: true);

		// логи
		Directory.CreateDirectory(Path.Combine(storage, "logs"));

		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(builder.Configuration)
			.CreateLogger();

		builder.Host.UseSerilog();

		// Json
		builder.Services.Configure<JsonOptions>(options =>
		{
			options.SerializerOptions.NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals;
			options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
		});

		// MVC
		builder.Services
			.AddControllers()
			.AddControllersAsServices();

		// Swagger
		builder.Services
			.AddSwaggerDocument((options, services) =>
			{
				var jsonOpts = services.GetRequiredService<IOptions<JsonOptions>>().Value;

				options.DocumentName = "Datalake App";
				options.Title = "Datalake App";
				options.Version = "v" + Version;

				options.SchemaSettings = new SystemTextJsonSchemaGeneratorSettings
				{
					SchemaType = NJsonSchema.SchemaType.OpenApi3,
					GenerateEnumMappingDescription = true,
					UseXmlDocumentation = true,
					SerializerOptions = jsonOpts.SerializerOptions,
				};
			})
			.AddEndpointsApiExplorer();

		// костыль, без которого LinqToDB не хочет работать с JSONB. Я не нашел, как передать эту настройку иначе
#pragma warning disable CS0618
		NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
#pragma warning restore CS0618

		// получаем строку подключения
		var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";

		// заполняем все указанные в ней переменные окружения реальными значениями
		connectionString = EnvExpander.FillEnvVariables(connectionString);
		Log.Information("ConnectionString: " + connectionString);

		// БД
		builder.Services
			.AddNpgsqlDataSource(connectionString)
			.AddDbContext<DatalakeEfContext>(options => options
				.UseNpgsql(connectionString))
			.AddLinqToDBContext<DatalakeContext>((provider, options) =>
			{
				var ms = new MappingSchema();
				ms.SetConverter<string, List<TagThresholdInfo>>(json => JsonSerializer.Deserialize<List<TagThresholdInfo>>(json)!);

				return options
					.UseMappingSchema(ms)
					.UseDefaultLogging(provider)
					.UseTraceLevel(System.Diagnostics.TraceLevel.Info)
					.UsePostgreSQL(connectionString);
			});

		// хранилища данный
		builder.Services.AddSingleton<DatalakeDataStore>(); // стейт-менеджер исходных данных
		builder.Services.AddSingleton<DatalakeDerivedDataStore>(); // стейт-менеджер зависимых данных
		builder.Services.AddSingleton<DatalakeCurrentValuesStore>(); // кэш последних значений
		builder.Services.AddSingleton<DatalakeEnergoIdStore>(); // хранилище данных пользователей из EnergoId

		// репозитории в памяти
		builder.Services.AddScoped<SettingsMemoryRepository>();
		builder.Services.AddScoped<AccessRightsMemoryRepository>();
		builder.Services.AddScoped<BlocksMemoryRepository>();
		builder.Services.AddScoped<SourcesMemoryRepository>();
		builder.Services.AddScoped<TagsMemoryRepository>();
		builder.Services.AddScoped<UserGroupsMemoryRepository>();
		builder.Services.AddScoped<UsersMemoryRepository>();

		// репозитории только БД
		builder.Services.AddScoped<AuditRepository>();
		builder.Services.AddScoped<ValuesRepository>();

		// сервис получения данных
		builder.Services.AddSingleton<ReceiverService>();

		// мониторинг активности
		builder.Services.AddSingleton<SessionManagerService>();
		builder.Services.AddSingleton<SourcesStateService>();
		builder.Services.AddSingleton<UsersStateService>();
		builder.Services.AddSingleton<TagsStateService>();
		builder.Services.AddSingleton<RequestsStateService>();

		// система сбора данных
		builder.Services.AddSingleton<CollectorWriter>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<CollectorWriter>());
		builder.Services.AddHostedService<CollectorProcessor>();
		builder.Services.AddSingleton<CollectorFactory>();

		// работа с пользователями
		builder.Services.AddSingleton<AuthenticationService>();

		// обновление настроек
		builder.Services.AddSingleton<SettingsHandlerService>();
		builder.Services.AddHostedService<SettingsHandlerService>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<SettingsHandlerService>());

		// обновление данных из EnergoId
		builder.Services.AddHostedService(provider => provider.GetRequiredService<DatalakeEnergoIdStore>());

		// настройка БД
		builder.Services.AddSingleton<DbInitializer>();
		builder.Services.AddSingleton<DbExternalInitializer>();

		// обработчики
		builder.Services.AddTransient<AuthMiddleware>();
		builder.Services.AddTransient<SentryRequestBodyMiddleware>();

		// оповещения об ошибках
		var sentrySection = builder.Configuration.GetSection("Sentry");
		builder.WebHost.UseSentry(o =>
		{
			o.Environment = CurrentEnvironment;
			o.Dsn = sentrySection[nameof(o.Dsn)];
			o.Debug = bool.TryParse(sentrySection[nameof(o.Debug)], out var dbg) && dbg;
			o.Release = $"{builder.Environment.ApplicationName}@{Version}";
			o.TracesSampleRate = double.TryParse(sentrySection[nameof(o.TracesSampleRate)], out var rate) ? rate : 0.0;
		});

		// сборка
		var app = builder.Build();
		WebRootPath = app.Environment.WebRootPath;

		if (!app.Environment.IsProduction())
		{
			app
				.UseDeveloperExceptionPage()
				.UseOpenApi()
				.UseSwaggerUi();
		}

		app
			.UseSerilogRequestLogging(options =>
			{
				// шаблон одного сообщения на запрос
				options.MessageTemplate = "HTTP: [{Controller}.{Action}] > {StatusCode} in {Elapsed:0.0000} ms";

				// если упало — логируем Error, иначе Information
				options.GetLevel = (httpContext, elapsed, ex) =>
				httpContext.Request.Method == "OPTIONS"
					? LogEventLevel.Verbose
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
			})
			.UseSentryTracing()
			.UseDefaultFiles()
			.UseStaticFiles()
			.UseHttpsRedirection()
			.UseRouting()
			.UseCors(policy =>
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
			})
			.UseMiddleware<AuthMiddleware>()
			.UseMiddleware<SentryRequestBodyMiddleware>()
			.UseExceptionHandler(ErrorsMiddleware.ErrorHandler)
			.EnsureCorsMiddlewareOnError();

		app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");
		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");

		// запуск БД
		var thisDb = app.Services.GetRequiredService<DbInitializer>();
		await thisDb.DoAsync();

		var externalDb = app.Services.GetRequiredService<DbExternalInitializer>();
		await externalDb.DoAsync();

		// запуск веб-сервера
		Log.Information("Приложение запущено");
		await app.RunAsync();
	}
}
