using Datalake.Inventory.Api.Bootstrap;
using Datalake.Inventory.Application.Bootstrap;
using Datalake.Inventory.Infrastructure.Bootstrap;
using Datalake.PublicApi.Constants;
using Datalake.Shared.Hosting;
using Datalake.Shared.Hosting.Bootstrap;
using Datalake.Shared.Hosting.Middlewares;
using System.Reflection;

namespace Datalake.Inventory.Api;

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
		builder.AddApi();
		builder.AddInfrastructure();
		builder.AddApplication();

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
						AuthConstants.TokenHeader,
						AuthConstants.GlobalAccessHeader,
						AuthConstants.NameHeader,
						AuthConstants.UnderlyingUserGuidHeader,
					]);
			})
			.UseSharedSentryBodyWriter()
			.UseSharedCorsOnError();

		// установка роутинга
		app.MapApi();

		// запуск
		app.NotifyStart(nameof(Inventory), CurrentEnvironment, Version);
		await app.RunAsync();
	}
}