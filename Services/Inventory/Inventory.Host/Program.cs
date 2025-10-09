using Datalake.Inventory.Application;
using Datalake.Inventory.Infrastructure;
using Datalake.Inventory.Api;
using Datalake.Shared.Api.Constants;
using Datalake.Shared.Hosting;
using Datalake.Shared.Hosting.Bootstrap;
using Datalake.Shared.Hosting.Middlewares;
using System.Reflection;

namespace Datalake.Inventory.Host;

public class Program
{
	internal static string CurrentEnvironment { get; set; } = string.Empty;

	internal static VersionValue Version { get; set; } = new();

	public static async Task Main(string[] args)
	{
		// дефолт сообщение, чтобы увидеть факт запуска
		Console.WriteLine($"{nameof(Inventory)}: v{Version.Full()}");

		// настройка
		var builder = WebApplication.CreateBuilder(args);
		CurrentEnvironment = builder.Environment.EnvironmentName;

		builder.AddShared(CurrentEnvironment, Version, Assembly.GetCallingAssembly());
		builder.AddInfrastructure();
		builder.AddApplication();
		builder.AddApi();
		builder.AddHosting();

		// сборка
		var app = builder.Build();

		// пайп обработки запросов
		if (!app.Environment.IsProduction())
		{
			app
				.UseDeveloperExceptionPage()
				.UseOpenApi()
				.UseSwaggerUi();
		}

		app
			.UseSharedExceptionsHandler()
			.UseSentryTracing()
			.UseSharedSerilogRequestLogging()
			.UseHttpsRedirection()
			.UseRouting()
			.UseCors(policy =>
			{
				policy
					.AllowAnyMethod()
					.AllowAnyOrigin()
					.AllowAnyHeader()
					.WithExposedHeaders([
						Headers.UserGuidHeader,
						Headers.SessionTokenHeander,
					]);
			})
			.UseSharedSentryBodyWriter()
			.UseSharedCorsOnError();

		// установка роутинга
		app.UseApi();
		app.MapApi();

		// запуск
		app.NotifyStart(nameof(Inventory), CurrentEnvironment, Version);
		await app.RunAsync();
	}
}