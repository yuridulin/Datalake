using Datalake.Gateway.Host.Interfaces;
using Datalake.Gateway.Host.Services;
using Ocelot.DependencyInjection;

namespace Datalake.Gateway.Host;

public static class Bootstrap
{
	public static IHostApplicationBuilder AddHosting(this IHostApplicationBuilder builder)
	{
		// прокси через ocelot и общий swagger

		builder.Configuration
			.AddJsonFile($"ocelot.json", optional: false, reloadOnChange: true);

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

		builder.Services.AddScoped<ISessionTokenExtractor, SessionTokenExtractor>();

		return builder;
	}

	public static WebApplication UseApi(this WebApplication app)
	{
		app.MapHealthChecks("/health");

		return app;
	}

	public static WebApplication MapApi(this WebApplication app)
	{
		app.MapControllerRoute(
			name: "default",
			pattern: "{controller=Home}/{action=Index}/{id?}");
		app.MapFallbackToFile("{*path:regex(^(?!api).*$)}", "/index.html");

		return app;
	}
}
