using Datalake.ApiClasses.Exceptions;
using Datalake.Server.Constants;
using Datalake.Server.Services.SessionManager;
using Datalake.Server.Services.SessionManager.Models;
using System.Text;

namespace Datalake.Server.Middlewares;

/// <summary>
/// Обработчик, проверяющий аутентификацию
/// </summary>
/// <param name="sessionManager">Менеджер сессий доступа</param>
public class AuthMiddleware(SessionManagerService sessionManager) : IMiddleware
{
	/// <summary>
	/// Выполнение проверки аутентификации
	/// </summary>
	/// <param name="context">Контекст запроса</param>
	/// <param name="next">Следующий обработчик</param>
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		bool needToAuth = context.Request.Path.StartsWithSegments("/api") // только api
			&& (context.Request.Method != "OPTIONS") // только REST
			&& !(context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api/users/auth")) // не логин-пасс
			&& !(context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api/users/energo-id")); // не energoId

		AuthSession? authSession = null;
		if (needToAuth)
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
