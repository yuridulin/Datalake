using Datalake.Database.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Datalake.Server.Middlewares;

internal class ErrorsMiddleware
{
	internal static void ErrorHandler(IApplicationBuilder exceptionHandlerApp)
	{
		exceptionHandlerApp.Run(async context =>
		{
			var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();

			var error = exceptionHandlerPathFeature?.Error;
			string message;

			context.Response.ContentType = "text/plain; charset=UTF-8";

			if (error is DatabaseException database)
			{
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				await context.Response.WriteAsync(database.ToString());
			}
			else if (error is UnauthenticatedException unauthenticated)
			{
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				await context.Response.WriteAsync(unauthenticated.ToString());
			}
			else if (error is ForbiddenException forbidden)
			{
				context.Response.StatusCode = StatusCodes.Status403Forbidden;
				await context.Response.WriteAsync(forbidden.ToString());
			}
			else
			{
				context.Response.StatusCode = StatusCodes.Status500InternalServerError;
				message = "Ошибка выполнения на сервере" +
					"\n\n" + // разделитель, по которому клиент отсекает служебную часть сообщения
					error?.ToString() ?? "error is null";
				await context.Response.WriteAsync(message);
			}
		});
	}
}
