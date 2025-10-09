using Datalake.Shared.Api.Constants;
using Datalake.Shared.Hosting;
using Datalake.Shared.Hosting.Bootstrap;
using Datalake.Shared.Hosting.Middlewares;
using Ocelot.DependencyInjection;
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
		builder.Configuration
			.AddJsonFile($"ocelot.json", optional: false, reloadOnChange: true);

		// прокси через ocelot и общий swagger
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

		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");
		app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");

		app.NotifyStart(nameof(Gateway), CurrentEnvironment, Version);

		await app.UseOcelot();
		await app.RunAsync();
	}
}
