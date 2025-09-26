using Datalake.PrivateApi.Converters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

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
}
