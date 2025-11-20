using Datalake.Gateway.Host.Interfaces;
using Datalake.Gateway.Host.Proxy.Abstractions;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting;
using Datalake.Shared.Hosting.Constants;
using Datalake.Shared.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using NJsonSchema.Generation;

namespace Datalake.Gateway.Host;

/// <summary>
/// Расширение для настройки
/// </summary>
public static class Bootstrap
{
	/// <summary>
	/// Настройка хост-приложения
	/// </summary>
	public static IHostApplicationBuilder AddHosting(this IHostApplicationBuilder builder)
	{
		// MVC
		builder.Services
			.AddControllers()
			.AddControllersAsServices()
			.AddSharedJsonOptions();

		// Swagger
		builder.Services
			.AddSwaggerDocument((options, services) =>
			{
				options.Title = "Datalake";
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

		builder.Services.AddScoped<ISessionTokenExtractor, SessionTokenExtractor>();

		// Прокси
		builder.Services.Configure<IISServerOptions>(options =>
		{
			options.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
		});
		builder.Services.Configure<KestrelServerOptions>(options =>
		{
			options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
		});

		builder.Services
			.AddHttpClient("Data", (provider, client) =>
			{
				var baseAddress = builder.Configuration.GetSection("Services").GetSection("Data").Get<string>()
					?? throw new Exception("Адрес сервиса Data не получен");

				client.BaseAddress = new Uri(EnvExpander.FillEnvVariables(baseAddress));
			})
			.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
			{
				PooledConnectionLifetime = TimeSpan.FromMinutes(5),
				UseProxy = false,
				UseCookies = false
			});

		builder.Services
			.AddHttpClient("Inventory", (provider, client) =>
			{
				var baseAddress = builder.Configuration.GetSection("Services").GetSection("Inventory").Get<string>()
					?? throw new Exception("Адрес сервиса Inventory не получен");

				client.BaseAddress = new Uri(EnvExpander.FillEnvVariables(baseAddress));
			})
			.ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
			{
				PooledConnectionLifetime = TimeSpan.FromMinutes(5),
				UseProxy = false,
				UseCookies = false
			});

		builder.Services.AddSingleton<DataReverseProxyService>();
		builder.Services.AddSingleton<InventoryReverseProxyService>();

		return builder;
	}

	/// <summary>
	/// Настройка обработки запросов
	/// </summary>
	public static WebApplication MapApi(this WebApplication app)
	{
		// Обработка ошибок при проксировании запросов
		app.UseReverseProxyMiddleware();

		app.MapHealthChecks("/health");

		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");

		// Сброс на стартовую страницу клиента, если self-host
		app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");

		return app;
	}
}
