using MassTransit;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Datalake.GatewayService;

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
			.SetBasePath(configs)
			.AddJsonFile($"appsettings.json", optional: false, reloadOnChange: true)
			.AddJsonFile($"appsettings.{CurrentEnvironment}.json", optional: true, reloadOnChange: true)
			.AddJsonFile($"ocelot.json", optional: false, reloadOnChange: true);

		// прокси через ocelot и общий swagger
		builder.Services.AddControllers();
		builder.Services.AddEndpointsApiExplorer();
		builder.Services.AddSwaggerGen(c =>
		{
			c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
			{
				Title = "Datalake " + nameof(GatewayService),
				Version = "v1"
			});
		});

		builder.Services.AddOcelot(builder.Configuration);
		builder.Services.AddSwaggerForOcelot(builder.Configuration);

		// общение между сервисами
		var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");

		builder.Services.AddMassTransit(config =>
		{
			config.UsingRabbitMq((context, cfg) =>
			{
				cfg.Host(rabbitMqConfig["Host"], "/", h =>
				{
					h.Username(rabbitMqConfig["User"] ?? string.Empty);
					h.Password(rabbitMqConfig["Pass"] ?? string.Empty);
				});
			});
		});

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
