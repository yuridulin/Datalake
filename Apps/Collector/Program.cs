
using Datalake.Database.Constants;
using NJsonSchema.Generation;

namespace Datalake.Collector;

public class Program
{
	internal static string CurrentEnvironment { get; set; } = string.Empty;

	public static async Task Main(string[] args)
	{
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

		builder.Services
			.AddControllers()
			.AddControllersAsServices();

		// Swagger
		builder.Services
			.AddSwaggerDocument((options, services) =>
			{
				options.Title = "Datalake " + nameof(Collector);
				options.Version = "v1";

				options.SchemaSettings = new SystemTextJsonSchemaGeneratorSettings
				{
					SchemaType = NJsonSchema.SchemaType.OpenApi3,
					GenerateEnumMappingDescription = true,
					UseXmlDocumentation = true,
					SerializerOptions = Json.JsonSerializerOptions,
				};
			})
			.AddEndpointsApiExplorer();

		builder.Services.AddHealthChecks();

		var app = builder.Build();

		app.UseOpenApi();
		app.UseSwaggerUi();

		app.MapControllers();
		app.MapHealthChecks("/health");

		await app.RunAsync();
	}
}
