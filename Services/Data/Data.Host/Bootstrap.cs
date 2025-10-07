using Datalake.Data.Host.Services;
using Datalake.Shared.Hosting.Bootstrap;
using Datalake.Shared.Hosting.Interfaces;
using NJsonSchema.Generation;

namespace Datalake.Data.Host;

public static class BootstrapExtensions
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
				options.Title = "Datalake." + nameof(Data);
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

		return builder;
	}
}
