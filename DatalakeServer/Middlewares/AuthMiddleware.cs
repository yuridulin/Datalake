using DatalakeApiClasses.Exceptions;
using DatalakeServer.Constants;
using DatalakeServer.Services.SessionManager;
using DatalakeServer.Services.SessionManager.Models;
using System.Text;

namespace DatalakeServer.Middlewares;

/// <summary>
/// Обработчик, проверяющий аутентификацию
/// </summary>
/// <param name="sessionManager">Менеджер сессий доступа</param>
public class AuthMiddleware(SessionManagerService sessionManager) : IMiddleware
{
	string[] checkingMethods = ["GET", "POST", "PUT", "DELETE"];

	/// <summary>
	/// Выполнение проверки аутентификации
	/// </summary>
	/// <param name="context">Контекст запроса</param>
	/// <param name="next">Следующий обработчик</param>
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		bool isApi = context.Request.Path.StartsWithSegments("/api");
		bool isNotAuthApi = !context.Request.Path.StartsWithSegments("/api/users/auth")
			&& !context.Request.Path.StartsWithSegments("/api/users/energo-id");
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
