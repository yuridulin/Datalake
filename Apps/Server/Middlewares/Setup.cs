namespace Datalake.Server.Middlewares;

internal static class Setup
{
	internal static void AddMiddlewares(this WebApplicationBuilder builder)
	{
		builder.Services.AddTransient<AuthMiddleware>();
	}

	internal static void UseMiddlewares(this WebApplication app)
	{
		app.UseMiddleware<AuthMiddleware>();

		app.UseExceptionHandler(ErrorsMiddleware.ErrorHandler);

		app.EnsureCorsMiddlewareOnError();
	}
}
