using Datalake.Data.Api;
using Datalake.Data.Application;
using Datalake.Data.Infrastructure;
using Datalake.Shared.Hosting;
using Datalake.Shared.Hosting.Bootstrap;
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

		builder.AddShared(CurrentEnvironment, Version, Assembly.GetExecutingAssembly());
		builder.AddInfrastructure();
		builder.AddApplication();
		builder.AddApi();
		builder.AddHosting();

		// сборка
		var app = builder.Build();

		if (!app.Environment.IsProduction())
		{
			app.UseDeveloperExceptionPage();
			app.UseSwaggerUi();
		}

		app.UseOpenApi();
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
					// добавить для передачи пользователя
				]);
		});
		app.UseSharedSentryBodyWriter();
		app.UseSharedCorsOnError();

		app.UseApi();
		app.MapApi();

		await app.RunAsync();
	}
}
