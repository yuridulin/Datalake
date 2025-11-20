using Datalake.Domain.Exceptions;
using Datalake.Shared.Application.Exceptions;
using Datalake.Shared.Hosting.Constants;
using Datalake.Shared.Hosting.Converters;
using Datalake.Shared.Hosting.Middlewares;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using System.Reflection;

namespace Datalake.Shared.Hosting;

/// <summary>
/// Расширение программы
/// </summary>
public static class Bootstrap
{
	/// <summary>
	/// Добавление общих для всех сервисов настроек
	/// </summary>
	public static WebApplicationBuilder AddShared(this WebApplicationBuilder builder, string envName, VersionValue version, Assembly assembly)
	{
		// конфиг
		var storage = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "storage");
		var configs = Path.Combine(storage, "config");
		Directory.CreateDirectory(configs);
		builder.Configuration
			.SetBasePath(configs)
			.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{envName}.json", optional: true, reloadOnChange: true);

		// логи
		Directory.CreateDirectory(Path.Combine(storage, "logs"));

		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(builder.Configuration)
			.CreateLogger();

		builder.Host.UseSerilog();

		// Json
		builder.Services.Configure<JsonOptions>(options =>
		{
			options.SerializerOptions.NumberHandling = JsonSettings.JsonSerializerOptions.NumberHandling;
			options.SerializerOptions.PropertyNamingPolicy = JsonSettings.JsonSerializerOptions.PropertyNamingPolicy;
			options.SerializerOptions.Converters.Add(new NanToNullFloatConverter());
		});

		// обработчики
		builder.Services.AddTransient<SentryRequestBodyMiddleware>();

		// инициализатор работы
		builder.Services.AddHealthChecks();

		// оповещения об ошибках
		builder.UseSharedSentry(envName, version);

		// общение между сервисами
		builder.Services.AddSharedMassTransit(builder.Configuration, assembly);

		// MemoryCache для IUserAccessCache
		builder.Services.AddMemoryCache(options =>
		{
			options.SizeLimit = 10000; // Ограничение на количество записей
		});

		return builder;
	}

	/// <summary>
	/// Добавление общих настроек JSON
	/// </summary>
	public static IMvcBuilder AddSharedJsonOptions(
		this IMvcBuilder services)
	{
		return services.AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.NumberHandling = JsonSettings.JsonSerializerOptions.NumberHandling;
			options.JsonSerializerOptions.PropertyNamingPolicy = JsonSettings.JsonSerializerOptions.PropertyNamingPolicy;
			options.JsonSerializerOptions.Converters.Add(new NanToNullFloatConverter());
		});
	}

	/// <summary>
	/// Добавление общей настройки работы с сетевыми событиями
	/// </summary>
	public static IServiceCollection AddSharedMassTransit(
		this IServiceCollection services,
		IConfiguration configuration,
		Assembly? consumersAssembly = null)
	{
		var rabbitMqConfig = configuration.GetSection("RabbitMq");

		services.AddMassTransit(x =>
		{
			// Если сборка не указана — берём вызывающую
			consumersAssembly ??= Assembly.GetCallingAssembly();

			// Регистрируем всех consumers из сборки
			x.AddConsumers(consumersAssembly);

			x.UsingRabbitMq((context, cfg) =>
			{
				cfg.Host(rabbitMqConfig["Host"], "/", h =>
				{
					h.Username(rabbitMqConfig["User"] ?? string.Empty);
					h.Password(rabbitMqConfig["Pass"] ?? string.Empty);
				});

				// Автоматически создаст endpoints для всех consumers
				cfg.ConfigureEndpoints(context);
			});
		});

		return services;
	}

	/// <summary>
	/// Добавление общей работы с Sentry
	/// </summary>
	public static void UseSharedSentry(
		this WebApplicationBuilder builder,
		string environment,
		VersionValue version)
	{
		var sentrySection = builder.Configuration.GetSection("Sentry");

		builder.WebHost.UseSentry(o =>
		{
			o.Environment = environment;
			o.Dsn = sentrySection[nameof(o.Dsn)];
			o.Debug = bool.TryParse(sentrySection[nameof(o.Debug)], out var dbg) && dbg;
			o.Release = $"{builder.Environment.ApplicationName}@{version.Short()}";
			o.TracesSampleRate = double.TryParse(sentrySection[nameof(o.TracesSampleRate)], out var rate) ? rate : 0.0;

			// доменные и бизнес-ошибки, которые не нужны в Sentry
			o.AddExceptionFilterForType<ValidationException>();
			o.AddExceptionFilterForType<UnauthenticatedException>();
			o.AddExceptionFilterForType<UnauthorizedException>();
			o.AddExceptionFilterForType<ConflictException>();
			o.AddExceptionFilterForType<DomainException>();
			o.AddExceptionFilterForType<NotFoundException>();
		});
	}

	/// <summary>
	/// Добавление общей обработки ошибок ASP.NET
	/// </summary>
	public static IApplicationBuilder UseSharedExceptionsHandler(
		this IApplicationBuilder app)
	{
		return app.UseExceptionHandler(SharedExceptionsMiddleware.Handler);
	}

	/// <summary>
	/// Добавление общей обработки сообщений об ошибках в Serilog
	/// </summary>
	public static IApplicationBuilder UseSharedSerilogRequestLogging(
		this IApplicationBuilder app)
	{
		return app.UseSerilogRequestLogging(options =>
		{
			options.MessageTemplate = "Запрос API {Method} {Controller}.{Action}: статус {StatusCode} за {Elapsed:0} мс";

			options.GetLevel = (httpContext, elapsed, ex) =>
			{
				var path = httpContext.Request.Path.Value;

				if (path != null && path.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
					return LogEventLevel.Verbose;

				if (httpContext.Request.Method == "OPTIONS")
					return LogEventLevel.Verbose;

				if (ex != null || httpContext.Response.StatusCode >= 500)
					return LogEventLevel.Warning;

				return LogEventLevel.Debug;
			};

			options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
			{
				var endpoint = httpContext.GetEndpoint();
				var routePattern = endpoint?.Metadata.GetMetadata<RouteNameMetadata>();
				var actionDescriptor = endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>();

				diagnosticContext.Set("Method", httpContext.Request.Method);

				if (actionDescriptor != null)
				{
					diagnosticContext.Set("Controller", actionDescriptor.ControllerName);
					diagnosticContext.Set("Action", actionDescriptor.ActionName);
				}
				else
				{
					diagnosticContext.Set("Controller", "?");
					diagnosticContext.Set("Action", httpContext.Request.Path);
				}
			};
		});
	}

	/// <summary>
	/// Добавление общей записи тела запроса в сообщение об ошибке Sentry
	/// </summary>
	public static IApplicationBuilder UseSharedSentryBodyWriter(
		this IApplicationBuilder app)
	{
		return app.UseMiddleware<SentryRequestBodyMiddleware>();
	}

	/// <summary>
	/// Создание сообщения о запуске сервиса
	/// </summary>
	/// <param name="_"></param>
	/// <param name="name"></param>
	/// <param name="envName"></param>
	/// <param name="version"></param>
	public static void NotifyStart(
		this IApplicationBuilder _,
		string name,
		string envName,
		VersionValue version)
	{
		// отправка сообщения в Sentry, чтобы сразу засветить новый релиз
		string greetings = $"🚀 Приложение {name} запущено. Релиз: {envName}@{version.Short()}";
		SentrySdk.CaptureMessage(greetings, SentryLevel.Info);
		Log.Information(greetings);
	}
}
