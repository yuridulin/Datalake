using Datalake.Contracts;
using Datalake.Data.Application;
using Datalake.Data.Infrastructure;
using Datalake.Shared.Hosting;
using Datalake.Shared.Hosting.Bootstrap;
using Datalake.Shared.Hosting.Constants;
using Datalake.Shared.Hosting.Middlewares;
using System.Reflection;

namespace Datalake.Data.Host;

public class Program
{
	internal static string CurrentEnvironment { get; set; } = string.Empty;

	internal static VersionValue Version { get; set; } = new();

	public static async Task Main(string[] args)
	{
		// дефолт сообщение, чтобы увидеть факт запуска
		Console.WriteLine($"{nameof(Data)}: v{Version.Full()}");

		// настройка
		var builder = WebApplication.CreateBuilder(args);
		CurrentEnvironment = builder.Environment.EnvironmentName;

		// конфигурация
		builder.AddShared(CurrentEnvironment, Version, Assembly.GetExecutingAssembly());
		builder.AddInfrastructure();
		builder.AddApplication();
		builder.AddApi();
		builder.AddHosting();

		// сборка
		var app = builder.Build();

		// пайп обработки запросов
		if (!app.Environment.IsProduction())
		{
			app.UseDeveloperExceptionPage();
			app.UseOpenApi();
			app.UseSwaggerUi();
		}

		app.UseSharedExceptionsHandler();
		app.UseSentryTracing();
		app.UseSharedSerilogRequestLogging();
		app.UseHttpsRedirection();
		app.UseRouting();
		app.UseCors(policy =>
		{
			policy
				.AllowAnyMethod()
				.AllowAnyOrigin()
				.AllowAnyHeader()
				.WithExposedHeaders([
					Headers.UserGuidHeader,
					Headers.SessionTokenHeander,
				]);
		});
		app.UseSharedSentryBodyWriter();
		app.UseSharedCorsOnError();

		// установка роутинга
		app.MapApi();

		// запуск
		app.NotifyStart(nameof(Data), CurrentEnvironment, Version);
		await app.RunAsync();
	}
}
