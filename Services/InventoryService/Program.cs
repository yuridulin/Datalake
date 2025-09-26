using Datalake.Inventory.InMemory.Repositories;
using Datalake.InventoryService.Database;
using Datalake.InventoryService.Database.Repositories;
using Datalake.InventoryService.Initialization;
using Datalake.InventoryService.InMemory.Repositories;
using Datalake.InventoryService.InMemory.Stores;
using Datalake.InventoryService.Middlewares;
using Datalake.InventoryService.Services.Auth;
using Datalake.InventoryService.Services.Initialization;
using Datalake.InventoryService.Services.Maintenance;
using Datalake.InventoryService.Services.SettingsHandler;
using Datalake.PrivateApi.Middlewares;
using Datalake.PrivateApi.Settings;
using Datalake.PrivateApi.Utils;
using Datalake.PrivateApi.ValueObjects;
using Datalake.PublicApi.Constants;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Generation;
using Npgsql;
using Serilog;

namespace Datalake.InventoryService;

/// <summary>
/// Datalake Server
/// </summary>
public class Program
{
	internal static string WebRootPath { get; set; } = string.Empty;

	internal static string CurrentEnvironment { get; set; } = string.Empty;

	internal static VersionValue Version { get; set; } = new();

	/// <summary>
	/// Старт Datalake Server
	/// </summary>
	public static async Task Main(string[] args)
	{
		// дефолт сообщение, чтобы увидеть факт запуска
		Console.WriteLine($"{nameof(InventoryService)}: v{Version.Full()}");

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
		builder.Services.ConfigureCustomJsonOptions();

		// MVC
		builder.Services
			.AddControllers()
			.AddControllersAsServices()
			.AddCustomJsonOptions();

		// Swagger
		builder.Services
			.AddSwaggerDocument((options, services) =>
			{
				options.Title = "Datalake " + nameof(InventoryService);
				options.Version = "v" + Version;

				options.SchemaSettings = new SystemTextJsonSchemaGeneratorSettings
				{
					SchemaType = NJsonSchema.SchemaType.OpenApi3,
					GenerateEnumMappingDescription = true,
					UseXmlDocumentation = true,
					SerializerOptions = JsonSettings.JsonSerializerOptions,
				};
			})
			.AddEndpointsApiExplorer();

		// костыль, без которого LinqToDB не хочет работать с JSONB. Я не нашел, как передать эту настройку иначе
#pragma warning disable CS0618
		NpgsqlConnection.GlobalTypeMapper.EnableDynamicJson();
#pragma warning restore CS0618

		// БД
		var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";
		connectionString = EnvExpander.FillEnvVariables(connectionString);

		builder.Services
			.AddNpgsqlDataSource(connectionString)
			.AddDbContext<InventoryEfContext>(options => options
				.UseNpgsql(connectionString));

		// хранилища данный
		builder.Services.AddSingleton<DatalakeDataStore>(); // стейт-менеджер исходных данных
		builder.Services.AddSingleton<DatalakeAccessStore>(); // стейт-менеджер зависимых данных
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
		builder.Services.AddScoped<LogsRepository>();


		// мониторинг активности
		builder.Services.AddSingleton<UsersStateService>();

		// работа с пользователями
		builder.Services.AddSingleton<AuthenticationService>();

		// обновление настроек
		builder.Services.AddSingleton<SettingsHandlerService>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<SettingsHandlerService>());

		// обновление данных из EnergoId
		builder.Services.AddHostedService(provider => provider.GetRequiredService<DatalakeEnergoIdStore>());

		// настройка БД
		builder.Services.AddSingleton<DbInitializer>();
		builder.Services.AddSingleton<DbExternalInitializer>();

		// обработчики
		builder.Services.AddTransient<AuthMiddleware>();
		builder.Services.AddTransient<SentryRequestBodyMiddleware>();

		// инициализатор работы
		builder.Services.AddSingleton<LoaderService>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<LoaderService>());
		builder.Services.AddHealthChecks();

		// оповещения об ошибках
		var sentrySection = builder.Configuration.GetSection("Sentry");
		builder.WebHost.UseSentry(o =>
		{
			o.Environment = CurrentEnvironment;
			o.Dsn = sentrySection[nameof(o.Dsn)];
			o.Debug = bool.TryParse(sentrySection[nameof(o.Debug)], out var dbg) && dbg;
			o.Release = $"{builder.Environment.ApplicationName}@{Version.Short()}";
			o.TracesSampleRate = double.TryParse(sentrySection[nameof(o.TracesSampleRate)], out var rate) ? rate : 0.0;
		});

		// общение между сервисами
		builder.Services.AddCustomMassTransit(builder.Configuration, typeof(Program).Assembly);

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
			.UseExceptionHandler(ErrorsMiddleware.ErrorHandler)
			.UseSentryTracing()
			.UseDefaultFiles()
			.UseStaticFiles(new StaticFileOptions
			{
				OnPrepareResponse = (ctx) =>
				{
					if (ctx.File.Name == "index.html")
					{
						ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
						ctx.Context.Response.Headers.Append("Pragma", "no-cache");
						ctx.Context.Response.Headers.Append("Expires", "0");
					}
				}
			})
			.UseCustomSerilog()
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
			.EnsureCorsMiddlewareOnError();

		app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");
		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");
		app.MapHealthChecks("/health");

		// запуск БД
		var thisDb = app.Services.GetRequiredService<DbInitializer>();
		await thisDb.DoAsync();

		var externalDb = app.Services.GetRequiredService<DbExternalInitializer>();
		await externalDb.DoAsync();

		// отправка сообщения в Sentry, чтобы сразу засветить новый релиз
		string greetings = $"🚀 Приложение {nameof(InventoryService)} запущено. Релиз: {builder.Environment.ApplicationName}@{Version.Short()}";
		SentrySdk.CaptureMessage(greetings, SentryLevel.Info);
		Log.Information(greetings);

		// запуск веб-сервера
		await app.RunAsync();
	}
}