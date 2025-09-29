using Datalake.InventoryService.Api.Services;
using Datalake.InventoryService.Application.Features.Audit.Queries.Audit;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Infrastructure.Cache.EnergoId;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Cache.UserAccess;
using Datalake.InventoryService.Infrastructure.Database;
using Datalake.InventoryService.Infrastructure.Database.Initialization;
using Datalake.PrivateApi.Middlewares;
using Datalake.PrivateApi.Settings;
using Datalake.PrivateApi.Utils;
using Datalake.PrivateApi.ValueObjects;
using Datalake.PublicApi.Constants;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Generation;
using Serilog;
using System.Reflection;

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

		// БД
		var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";
		connectionString = EnvExpander.FillEnvVariables(connectionString);

		builder.Services
			.AddNpgsqlDataSource(connectionString)
			.AddDbContext<InventoryEfContext>(options => options
				.UseNpgsql(connectionString));

		// хранилища данный
		builder.Services.AddSingleton<InventoryCacheStore>(); // кэш состояния схемы данных
		builder.Services.AddSingleton<UserAccessCacheStore>(); // кэш вычисляемых прав доступа
		builder.Services.AddSingleton<EnergoIdCacheStore>(); // кэш данных пользователей EnergoId, обновляет вьюшку
		builder.Services.AddHostedService(provider => provider.GetRequiredService<EnergoIdCacheStore>());

		// черная магия для регистрации обработчиков
		builder.Services.Scan(scan => scan
			.FromAssemblies(Assembly.GetExecutingAssembly())
			.AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)))
				.AsImplementedInterfaces()
				.WithScopedLifetime());

		// репозитории только БД
		builder.Services.AddScoped<GetAuditQueryHandler>();

		// мониторинг активности
		builder.Services.AddSingleton<UsersStateService>();

		// работа с пользователями
		builder.Services.AddSingleton<AuthenticationService>();

		// обновление настроек
		builder.Services.AddSingleton<SettingsHandlerService>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<SettingsHandlerService>());

		// настройка БД
		builder.Services.AddSingleton<DbInitializer>();
		builder.Services.AddSingleton<DbExternalInitializer>();

		// обработчики
		builder.Services.AddTransient<SentryRequestBodyMiddleware>();

		// инициализатор работы
		builder.Services.AddSingleton<LoaderService>();
		builder.Services.AddHostedService(provider => provider.GetRequiredService<LoaderService>());
		builder.Services.AddHealthChecks();

		// оповещения об ошибках
		builder.UseCustomSentry(CurrentEnvironment, Version);
		
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