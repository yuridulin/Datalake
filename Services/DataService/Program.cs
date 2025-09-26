using Datalake.DataService.Abstractions;
using Datalake.DataService.Consumers;
using Datalake.DataService.Database;
using Datalake.DataService.Database.Interfaces;
using Datalake.DataService.Database.Repositories;
using Datalake.DataService.Factories;
using Datalake.DataService.Services.Auth;
using Datalake.DataService.Services.Collection;
using Datalake.DataService.Services.Metrics;
using Datalake.DataService.Services.Receiver;
using Datalake.DataService.Services.Values;
using Datalake.DataService.Stores;
using Datalake.PrivateApi.Middlewares;
using Datalake.PrivateApi.Settings;
using Datalake.PrivateApi.Utils;
using Datalake.PrivateApi.ValueObjects;
using LinqToDB;
using LinqToDB.AspNet;
using LinqToDB.AspNet.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using NJsonSchema.Generation;
using Serilog;

namespace Datalake.DataService;

public class Program
{
	internal static string CurrentEnvironment { get; set; } = string.Empty;

	internal static VersionValue Version { get; set; } = new();

	public static async Task Main(string[] args)
	{
		Console.WriteLine($"{nameof(DataService)}: v{Version.Full()}");

		// настройка
		var builder = WebApplication.CreateBuilder(args);
		CurrentEnvironment = builder.Environment.EnvironmentName;

		// конфигурация
		var storage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage");
		var configs = Path.Combine(storage, "config");
		builder.Configuration
			.SetBasePath(configs)
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{CurrentEnvironment}.json", optional: true, reloadOnChange: true);

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
				options.Title = "Datalake " + nameof(DataService);
				options.Version = "v1";

				options.SchemaSettings = new SystemTextJsonSchemaGeneratorSettings
				{
					SchemaType = NJsonSchema.SchemaType.OpenApi3,
					GenerateEnumMappingDescription = true,
					UseXmlDocumentation = true,
					SerializerOptions = JsonSettings.JsonSerializerOptions,
				};
			})
			.AddEndpointsApiExplorer();

		// статус
		builder.Services.AddHealthChecks();

		// общение между сервисами
		var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");

		builder.Services.AddMassTransit(config =>
		{
			config.AddConsumer<SomethingHappenedConsumer>();

			config.UsingRabbitMq((context, cfg) =>
			{
				cfg.Host(rabbitMqConfig["Host"], "/", h =>
				{
					h.Username(rabbitMqConfig["User"] ?? string.Empty);
					h.Password(rabbitMqConfig["Pass"] ?? string.Empty);
				});

				// Настройка получения сообщений
				cfg.ReceiveEndpoint("something-happened", e =>
				{
					e.ConfigureConsumer<SomethingHappenedConsumer>(context);

					// Опционально: привязка к определенному exchange
					e.Bind("something-happened");
				});
			});
		});

		// БД
		var connectionString = builder.Configuration.GetConnectionString("Default") ?? "";
		connectionString = EnvExpander.FillEnvVariables(connectionString);

		builder.Services
			.AddDbContext<DataEfContext>(options => options
				.UseNpgsql(connectionString))
			.AddLinqToDBContext<DataLinqToDbContext>((provider, options) =>
			{
				return options
					.UseDefaultLogging(provider)
					.UseTraceLevel(System.Diagnostics.TraceLevel.Verbose)
					.UsePostgreSQL(connectionString);
			});

		// сторы
		builder.Services.AddSingleton<IAccessStore, AccessStore>();
		builder.Services.AddSingleton<ITagsStore, TagsStore>();
		builder.Services.AddSingleton<ISourcesStore, SourcesStore>();
		builder.Services.AddSingleton<ICurrentValuesStore, CurrentValuesStore>();

		// сервисы
		builder.Services.AddSingleton<IAuthenticatorService, AuthenticationService>();
		builder.Services.AddSingleton<IReceiverService, ReceiverService>();
		builder.Services.AddSingleton<IGetValuesService, GetValuesService>();
		builder.Services.AddSingleton<IManualWriteValuesService, ManualWriteValuesService>();
		builder.Services.AddSingleton<ISystemWriteValuesService, SystemWriteValuesService>();

		builder.Services.AddSingleton<ITagHistoryFactory, TagHistoryFactory>();
		builder.Services.AddSingleton<ICollectorFactory, CollectorFactory>();

		builder.Services.AddSingleton<RequestsStateService>();
		builder.Services.AddSingleton<SourcesStateService>();
		builder.Services.AddSingleton<TagsReceiveStateService>();
		builder.Services.AddSingleton<TagsStateService>();

		builder.Services.AddScoped<IWriteHistoryRepository, WriteHistoryRepository>();
		builder.Services.AddScoped<IGetHistoryRepository, GetHistoryRepository>();
		builder.Services.AddScoped<IGetAggregatedHistoryRepository, GetAggregatedHistoryRepository>();

		// службы
		builder.Services.AddSingleton<ICollectorProcessor, CollectorProcessor>();
		builder.Services.AddSingleton<ICollectorWriter, CollectorWriter>();

		builder.Services.AddHostedService(provider => provider.GetRequiredService<ICollectorProcessor>());
		builder.Services.AddHostedService(provider => provider.GetRequiredService<ICollectorWriter>());

		// обработчики
		builder.Services.AddTransient<SentryRequestBodyMiddleware>();

		// sentry
		var sentrySection = builder.Configuration.GetSection("Sentry");
		builder.WebHost.UseSentry(o =>
		{
			o.Environment = CurrentEnvironment;
			o.Dsn = sentrySection[nameof(o.Dsn)];
			o.Debug = bool.TryParse(sentrySection[nameof(o.Debug)], out var dbg) && dbg;
			o.Release = $"{builder.Environment.ApplicationName}@{Version.Short()}";
			o.TracesSampleRate = double.TryParse(sentrySection[nameof(o.TracesSampleRate)], out var rate) ? rate : 0.0;
		});

		// сборка
		var app = builder.Build();

		app.UseOpenApi();
		app.UseSwaggerUi();
		app.UseSentryTracing();
		app.UseCustomSerilog();
		app.UseCors(policy =>
		{
			policy.AllowAnyMethod();
			policy.AllowAnyOrigin();
			policy.AllowAnyHeader();
			policy.WithExposedHeaders([
						
			]);
		});
		app.UseMiddleware<SentryRequestBodyMiddleware>();
		app.EnsureCorsMiddlewareOnError();

		app.MapControllers();
		app.MapHealthChecks("/health");

		// отправка сообщения в Sentry, чтобы сразу засветить новый релиз
		string greetings = $"🚀 Приложение {nameof(DataService)} запущено. Релиз: {builder.Environment.ApplicationName}@{Version.Short()}";
		SentrySdk.CaptureMessage(greetings, SentryLevel.Info);
		Log.Information(greetings);

		await app.RunAsync();
	}
}
