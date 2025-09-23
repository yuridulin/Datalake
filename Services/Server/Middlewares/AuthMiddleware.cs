using Datalake.Database.Constants;
using Datalake.Database.Extensions;
using Datalake.Database.InMemory.Stores.Derived;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.Auth;
using Datalake.Server.Services.Maintenance;

namespace Datalake.Server.Middlewares;

/// <summary>
/// Обработчик, проверяющий аутентификацию
/// </summary>
public class AuthMiddleware(
	DatalakeSessionsStore sessionsStore,
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
			authSession = await sessionsStore.GetExistSessionAsync(context);
			if (authSession == null)
			{
				context.Response.StatusCode = 401;
				var token = context.Request.Headers[AuthConstants.TokenHeader];
				throw Errors.NoAccessToken(token);
			}

			context.Response.AddSessionToResponse(authSession);
			stateService.WriteVisit(authSession.UserGuid);
		}

		context.Items.Add(AuthConstants.ContextSessionKey, authSession?.AuthInfo);

		await next.Invoke(context);
	}
}
