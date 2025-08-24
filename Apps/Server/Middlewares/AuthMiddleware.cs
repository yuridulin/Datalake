using Datalake.Database.Constants;
using Datalake.PublicApi.Constants;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.Auth.Models;
using Datalake.Server.Services.Maintenance;
using System.Text;

namespace Datalake.Server.Middlewares;

/// <summary>
/// Обработчик, проверяющий аутентификацию
/// </summary>
public class AuthMiddleware(
	SessionManagerService sessionManager,
	UsersStateService stateService) : IMiddleware
{
	static readonly byte[] ErrorMessage = Encoding.UTF8.GetBytes("Access Denied - No Auth");

	/// <summary>
	/// Выполнение проверки аутентификации
	/// </summary>
	/// <param name="context">Контекст запроса</param>
	/// <param name="next">Следующий обработчик</param>
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		bool needToAuth =
			// только api
			context.Request.Path.StartsWithSegments("/api")
			// только REST
			&& (context.Request.Method != "OPTIONS")
			// не логин-пасс
			&& !(context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api/users/auth"))
			// не energoId
			&& !(context.Request.Method == "POST" && context.Request.Path.StartsWithSegments("/api/users/energo-id"));

		AuthSession? authSession = null;
		if (needToAuth)
		{
			authSession = sessionManager.GetExistSession(context);
			if (authSession == null)
			{
				context.Response.StatusCode = 401;
				var token = context.Request.Headers[AuthConstants.TokenHeader];
				throw Errors.NoAccessToken(token);
			}
			sessionManager.AddSessionToResponse(authSession, context.Response);
			stateService.WriteVisit(authSession.UserGuid);
		}

		context.Items.Add(AuthConstants.ContextSessionKey, authSession?.AuthInfo);

		await next.Invoke(context);
	}
}
