using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Datalake.Gateway;

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
		var configs = Path.Combine(storage, "config");
		builder.Configuration
			.SetBasePath(storage)
			.AddJsonFile(Path.Combine(configs, $"appsettings.json"), optional: false, reloadOnChange: true)
			.AddJsonFile(Path.Combine(configs, $"appsettings.{CurrentEnvironment}.json"), optional: true, reloadOnChange: true)
			.AddJsonFile(Path.Combine(configs, $"ocelot.json"), optional: false, reloadOnChange: true);

		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
			{
				Title = "Datalake " + nameof(Gateway),
				Version = "v1"
			});
		});

		builder.Services.AddOcelot(builder.Configuration);
		builder.Services.AddSwaggerForOcelot(builder.Configuration);

		var app = builder.Build();

		app.UseSwagger();
		app.UseSwaggerForOcelotUI(opt =>
		{
			opt.PathToSwaggerGenerator = "/swagger/docs";
		});

		await app.UseOcelot();
		await app.RunAsync();
	}
}
