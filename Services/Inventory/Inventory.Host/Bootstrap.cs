using Datalake.Inventory.Host.Services;
using Datalake.Shared.Hosting.Bootstrap;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using NJsonSchema.Generation;
using System.Net;

namespace Datalake.Inventory.Host;

public static class Bootstrap
{
	public static IHostApplicationBuilder AddHosting(this WebApplicationBuilder builder)
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
				options.Title = "Datalake." + nameof(Inventory);
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

		builder.Services.AddSingleton<IAuthenticator, AuthenticationService>();

		builder.Services.AddGrpc(options =>
		{
			options.EnableDetailedErrors = true;
		});

		builder.WebHost.ConfigureKestrel(options =>
		{
			// Порт для REST (HTTP/1.1)
			options.Listen(IPAddress.Any, 8080, o =>
			{
				o.Protocols = HttpProtocols.Http1;
			});

			// Порт для gRPC (HTTP/2)
			options.Listen(IPAddress.Any, 5000, o =>
			{
				o.Protocols = HttpProtocols.Http2;
			});
		});

		return builder;
	}

	public static WebApplication MapApi(this WebApplication app)
	{
		app.MapHealthChecks("/health");

		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");

		app.MapGrpcService<InventoryGrpcServer>();

		return app;
	}
}
