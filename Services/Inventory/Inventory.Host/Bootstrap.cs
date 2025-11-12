using Datalake.Inventory.Host.Services;
using Datalake.Shared.Hosting.Bootstrap;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using NJsonSchema.Generation;
using NSwag;
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

		app.MapGet("/swagger/datalake.json", async (HttpClient http) =>
		{
			var docs = new List<OpenApiDocument>();

			// 1. Swagger самого Gateway
			using var gatewayStream = File.OpenRead("swagger/v1/swagger.json");
			var reader = new OpenApiStreamReader();
			var gatewayDoc = reader.Read(gatewayStream, out _);
			docs.Add(gatewayDoc);

			// 2. Downstream сервисы
			var urls = new[]
			{
				"http://localhost:5001/swagger/v1/swagger.json",
				"http://localhost:5002/swagger/v1/swagger.json"
			};

			foreach (var url in urls)
			{
				var json = await http.GetStringAsync(url);
				var doc = new OpenApiStringReader().Read(json, out _);
				docs.Add(doc);
			}

			// 3. Объединение
			var merged = new OpenApiDocument
			{
				Info = new OpenApiInfo { Title = "Unified API", Version = "v1" },
				Paths = new OpenApiPaths(),
				Components = new OpenApiComponents()
			};

			foreach (var doc in docs)
			{
				foreach (var path in doc.Paths)
				{
					// Добавляем префикс, чтобы не пересекались
					var prefixedPath = "/"
							+ (doc.Info.Title?.ToLower() ?? "service")
							+ path.Key;
					merged.Paths[prefixedPath] = path.Value;
				}

				foreach (var schema in doc.Components.Schemas)
				{
					if (!merged.Components.Schemas.ContainsKey(schema.Key))
						merged.Components.Schemas[schema.Key] = schema.Value;
				}
			}

			// 4. Вернуть JSON
			var writer = new OpenApiJsonWriter(new StringWriter());
			merged.SerializeAsV3(writer);
			return Results.Text(writer.GetStringBuilder().ToString(), "application/json");
		});

		return app;
	}
}
