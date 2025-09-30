using Datalake.PrivateApi.Converters;
using Datalake.PrivateApi.Exceptions;
using Datalake.PrivateApi.ValueObjects;
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

namespace Datalake.PrivateApi.Settings;

public static class ProgramExtensions
{
	public static IServiceCollection ConfigureCustomJsonOptions(this IServiceCollection services)
	{
		return services.Configure<JsonOptions>(options =>
		{
			options.SerializerOptions.NumberHandling = JsonSettings.JsonSerializerOptions.NumberHandling;
			options.SerializerOptions.PropertyNamingPolicy = JsonSettings.JsonSerializerOptions.PropertyNamingPolicy;
			options.SerializerOptions.Converters.Add(new NanToNullFloatConverter());
		});
	}

	public static IMvcBuilder AddCustomJsonOptions(this IMvcBuilder services)
	{
		return services.AddJsonOptions(options =>
		{
			options.JsonSerializerOptions.NumberHandling = JsonSettings.JsonSerializerOptions.NumberHandling;
			options.JsonSerializerOptions.PropertyNamingPolicy = JsonSettings.JsonSerializerOptions.PropertyNamingPolicy;
			options.JsonSerializerOptions.Converters.Add(new NanToNullFloatConverter());
		});
	}

	public static IApplicationBuilder UseCustomSerilog(this IApplicationBuilder app)
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
	/// Регистрирует MassTransit с RabbitMQ и автоматически подключает все Consumers из указанной сборки.
	/// </summary>
	public static IServiceCollection AddCustomMassTransit(
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

	public static void UseCustomSentry(
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
}
