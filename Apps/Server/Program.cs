using Datalake.Database;
using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Repositories;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
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
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Generation;
using Npgsql;
using Serilog;
using Serilog.Events;
using System.Reflection;
using System.Text.Json;

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
	public static void Main(string[] args)
	{
		// дефолт сообщение, чтобы увидеть факт запуска
		Console.WriteLine($"Datalake: v{Version}");

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

		// MVC
		builder.Services
			.AddControllers()
			.AddControllersAsServices()
			.AddJsonOptions(options =>
			{
				options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowNamedFloatingPointLiterals;
			});

		// сервисы
		builder.Services
			.AddSwaggerDocument((options, services) =>
			{
				options.DocumentName = "Datalake App";
				options.Title = "Datalake App";
				options.Version = "v" + Version;

				options.SchemaSettings = new SystemTextJsonSchemaGeneratorSettings
				{
					GenerateEnumMappingDescription = true,
					UseXmlDocumentation = true,
				};
				options.SchemaSettings.SchemaProcessors.Add(new XEnumVarnamesNswagSchemaProcessor());
			})
			.AddEndpointsApiExplorer();

		// костыль, без которого LinqToDB не хочет работать с JSONB. Я не нашел, как передать эту настройку иначе
#pragma warning disable CS0618
		NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
#pragma warning restore CS0618

		// получаем строку подключения
		var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";

		// заполняем все указанные в ней переменные окружения реальными значениями
		connectionString = FillEnvVariables(connectionString);
		Log.Information("ConnectionString: " + connectionString);

		// БД
		builder.Services
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
		builder.Services.AddScoped<AuthenticationService>();

		// обновление настроек
		builder.Services.AddSingleton<SettingsHandlerService>();
		builder.Services.AddHostedService<SettingsHandlerService>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<SettingsHandlerService>());

		// обработчики
		builder.Services.AddTransient<AuthMiddleware>();

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
			app.UseDeveloperExceptionPage();
			app.UseOpenApi();
			app.UseSwaggerUi();
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
			.UseExceptionHandler(ErrorsMiddleware.ErrorHandler)
			.EnsureCorsMiddlewareOnError();

		app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");
		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");

		// запуск БД
		StartWorkWithDatabase(app);

		// запуск веб-сервера
		app.Run();
	}

	private static string FillEnvVariables(string sourceString)
	{
		var env = Environment.GetEnvironmentVariables();
		foreach (var part in sourceString.Split("${"))
		{
			int endSymbol = part.IndexOf('}');
			if (!part.Contains('}'))
				continue;

			string variable = part[..endSymbol];
			var value = env.Contains(variable!) ? env[variable!]?.ToString() : null;
			if (string.IsNullOrEmpty(value))
				throw new Exception("Expected ENV variable is not found: " + variable);
			else
				sourceString = sourceString.Replace($"${{{variable}}}", value);
		}

		return sourceString;
	}

	private static async void StartWorkWithDatabase(WebApplication app)
	{
		using var serviceScope = app.Services.GetService<IServiceScopeFactory>()?.CreateScope()
			?? throw new Exception("Серьезно?");

		// выполняем миграции через EF, хоть тут сгодится
		var ef = serviceScope.ServiceProvider.GetRequiredService<DatalakeEfContext>();
		ef.Database.Migrate();

		// теперь репозитории в отдельной DLL используют настроенный логгер
		DatalakeContext.LoggerFactory = app.Services.GetRequiredService<ILoggerFactory>();
		DatalakeContext.SetupLinqToDB();

		var db = serviceScope.ServiceProvider.GetRequiredService<DatalakeContext>();
		var dataStore = serviceScope.ServiceProvider.GetRequiredService<DatalakeDataStore>();
		var usersRepository = serviceScope.ServiceProvider.GetRequiredService<UsersMemoryRepository>();

		// начальное наполнение БД
		await db.EnsureDataCreatedAsync(dataStore, usersRepository);

		// как-то криво, но пишем аудит
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
