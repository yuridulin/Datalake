using Datalake.Gateway.Api;
using Datalake.Gateway.Application;
using Datalake.Gateway.Infrastructure;
using Datalake.Shared.Api.Constants;
using Datalake.Shared.Hosting;
using Datalake.Shared.Hosting.Bootstrap;
using Datalake.Shared.Hosting.Middlewares;
using Ocelot.Middleware;
using System.Reflection;

namespace Datalake.Gateway.Host;

public class Program
{
	internal static string CurrentEnvironment { get; set; } = string.Empty;

	internal static VersionValue Version { get; set; } = new();

	public static async Task Main(string[] args)
	{
		// дефолт сообщение, чтобы увидеть факт запуска
		Console.WriteLine($"{nameof(Gateway)}: v{Version.Full()}");

		// настройка
		var builder = WebApplication.CreateBuilder(args);
		CurrentEnvironment = builder.Environment.EnvironmentName;

		// конфигурация
		builder.AddShared(CurrentEnvironment, Version, Assembly.GetCallingAssembly());
		builder.AddInfrastructure();
		builder.AddApplication();
		builder.AddApi();
		builder.AddHosting();

		var app = builder.Build();

		if (!app.Environment.IsProduction())
		{
			app.UseDeveloperExceptionPage();
		}

		app.UseSwagger();
		app.UseSwaggerForOcelotUI(opt =>
		{
			opt.PathToSwaggerGenerator = "/swagger/docs";
		});

		app
			.UseSharedExceptionsHandler()
			.UseSentryTracing()
			.UseSharedSerilogRequestLogging()
			.UseHttpsRedirection()
			.UseRouting()
			.UseDefaultFiles()
			.UseStaticFiles(new StaticFileOptions
			{
				OnPrepareResponse = (ctx) =>
				{
					if (ctx.File.Name == "index.html")
					{
						ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
						ctx.Context.Response.Headers.Append("Pragma", "no-cache");
						ctx.Context.Response.Headers.Append("Expires", "0");
					}
				}
			})
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

		app.UseApi();
		app.MapApi();

		app.NotifyStart(nameof(Gateway), CurrentEnvironment, Version);

		await app.UseOcelot();
		await app.RunAsync();
	}
}
