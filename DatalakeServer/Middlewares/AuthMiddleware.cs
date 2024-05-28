using DatalakeApiClasses.Exceptions;
using DatalakeServer.Constants;
using DatalakeServer.Services.SessionManager;
using DatalakeServer.Services.SessionManager.Models;
using System.Text;

namespace DatalakeServer.Middlewares;

public class AuthMiddleware(SessionManagerService sessionManager) : IMiddleware
{
	string[] checkingMethods = ["GET", "POST", "PUT", "DELETE"];

	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		bool isApi = context.Request.Path.StartsWithSegments("/api");
		bool isNotAuthApi = !context.Request.Path.StartsWithSegments("/api/users/auth");
		bool isMethodInCheckedList = checkingMethods.Contains(context.Request.Method);

		AuthSession? authSession = null;
		if (isApi && isNotAuthApi && isMethodInCheckedList)
		{
			authSession = sessionManager.GetExistSession(context);
			if (authSession == null)
			{
				context.Response.StatusCode = 403;
				await context.Response.Body.WriteAsync(
					Encoding.UTF8.GetBytes(
						new ForbiddenException(message: "пользователь не аутентифицирован").ToString()));
				return;
			}
		}

		context.Items.Add(AuthConstants.ContextSessionKey, authSession?.User);

		await next.Invoke(context);
	}
}
