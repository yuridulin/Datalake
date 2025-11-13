using Datalake.Gateway.Host.Interfaces;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Bootstrap;
using Datalake.Shared.Infrastructure;
using NJsonSchema.Generation;

namespace Datalake.Gateway.Host;

public static class Bootstrap
{
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

		builder.Services.AddHttpClient("Data", (sp, c) =>
		{
			var baseAddress = builder.Configuration.GetSection("Services").GetSection("Data").Get<string>()
				?? throw new Exception("Адрес сервиса Data не получен");

			c.BaseAddress = new Uri(EnvExpander.FillEnvVariables(baseAddress));
		});

		builder.Services.AddHttpClient("Inventory", (sp, c) =>
		{
			var baseAddress = builder.Configuration.GetSection("Services").GetSection("Inventory").Get<string>()
				?? throw new Exception("Адрес сервиса Inventory не получен");

			c.BaseAddress = new Uri(EnvExpander.FillEnvVariables(baseAddress));
		});

		builder.Services.AddSingleton<DataReverseProxyService>();

		return builder;
	}

	public static WebApplication MapApi(this WebApplication app)
	{
		app.MapHealthChecks("/health");

		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");

		app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");


		return app;
	}
}
