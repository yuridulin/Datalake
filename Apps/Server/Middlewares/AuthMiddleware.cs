using Datalake.Database.Constants;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.Auth;
using Datalake.Server.Services.Auth;
using Datalake.Server.Services.Maintenance;

namespace Datalake.Server.Middlewares;

/// <summary>
/// Обработчик, проверяющий аутентификацию
/// </summary>
public class AuthMiddleware(
	SessionManagerService sessionManager,
	UsersStateService stateService) : IMiddleware
{
	/// <summary>
	/// Выполнение проверки аутентификации
	/// </summary>
	/// <param name="context">Контекст запроса</param>
	/// <param name="next">Следующий обработчик</param>
	public async Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		// определяем, нужно ли нам проверять
		bool needToAuth =
			// если api
			context.Request.Path.StartsWithSegments($"/{Defaults.ApiRoot}")
			// если REST
			&& (context.Request.Method != "OPTIONS")
			// и если не контроллер аутентификации
			&& !(context.Request.Method == HttpMethod.Post.Method && context.Request.Path.StartsWithSegments($"/{Defaults.ApiRoot}/{AuthControllerBase.ControllerRoute}"));

		UserSessionInfo? authSession = null;
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
