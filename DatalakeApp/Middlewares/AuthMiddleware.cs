using DatalakeApp.Services.SessionManager;
using DatalakeDatabase.Exceptions;
using System.Text;

namespace DatalakeApp.Middlewares;

public class AuthMiddleware(SessionManagerService sessionManager) : IMiddleware
{
	string[] checkingMethods = ["GET", "POST", "PUT", "DELETE"];

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		bool isApi = context.Request.Path.StartsWithSegments("/api");
		bool isNotAuthApi = !context.Request.Path.StartsWithSegments("/api/users/auth");
		bool isMethodInCheckedList = checkingMethods.Contains(context.Request.Method);

		if (isApi && isNotAuthApi && isMethodInCheckedList)
		{
			var session = sessionManager.GetExistSession(context);
			if (session == null)
			{
				context.Response.StatusCode = 403;
				await context.Response.Body.WriteAsync(
					Encoding.UTF8.GetBytes(
						new ForbiddenException(message: "пользователь не прошел авторизацию").ToString()));
				return;
			}
		}

		await next.Invoke(context);
	}
}
