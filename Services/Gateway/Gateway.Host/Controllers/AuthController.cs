using Datalake.Gateway.Api.Models.Auth;
using Datalake.Gateway.Api.Models.Sessions;
using Datalake.PublicApi.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.GatewayService.Controllers;

/// <summary>
/// Управление сессиями, логин/логаут
/// </summary>
public class AuthController : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Post" />: Аутентификация локального пользователя по связке "имя для входа/пароль"
	/// </summary>
	/// <param name="request">Данные для входа</param>
	/// <returns>Данные о учетной записи</returns>
	public async Task<ActionResult<UserSessionInfo>> AuthenticateLocalAsync(
		[BindRequired, FromBody] AuthLoginPassRequest request)
	{
		var userAuthInfo = authenticator.Authenticate(loginPass);

		var session = await sessionsStore.OpenSessionAsync(userAuthInfo, UserType.Local);
		AddSessionToResponse(session, Response);

		return await Task.FromResult(session);
	}

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Аутентификация пользователя, прошедшего проверку на сервере EnergoId
	/// </summary>
	/// <param name="request">Данные пользователя Keycloak</param>
	/// <returns>Данные о учетной записи</returns>
	public async Task<ActionResult<UserSessionInfo>> AuthenticateEnergoIdUserAsync(
		[BindRequired, FromBody] AuthEnergoIdRequest request)
	{
		var userAuthInfo = authenticator.Authenticate(energoIdInfo);

		var session = await sessionsStore.OpenSessionAsync(userAuthInfo, UserType.EnergoId);
		AddSessionToResponse(session, Response);

		return await Task.FromResult(session);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение информации о учетной записи на основе текущей сессии
	/// </summary>
	/// <returns>Данные о учетной записи</returns>
	public async Task<ActionResult<UserSessionInfo?>> IdentifyAsync()
	{
		authenticator.Authenticate(HttpContext);
		var session = await sessionsStore.GetExistSessionAsync(HttpContext);
		if (session != null)
		{
			AddSessionToResponse(session, Response);
		}
		return session;
	}

	/// <summary>
	/// <see cref="HttpMethod.Delete"/>: Закрытие уканной сессии пользователя
	/// </summary>
	/// <param name="token">Сессионный токен доступа</param>
	public async Task<ActionResult> LogoutAsync(
		[BindRequired, FromQuery] string token)
	{
		authenticator.Authenticate(HttpContext);

		await sessionsStore.CloseSessionAsync(token);

		return NoContent();
	}


	/// <summary>
	/// Добавление данных о сессии к запросу
	/// </summary>
	/// <param name="session">Сессия</param>
	/// <param name="response">Запрос</param>
	private static void AddSessionToResponse(UserSessionInfo session, HttpResponse response)
	{
		response.Headers[AuthConstants.TokenHeader] = session.Token;
		response.Headers[AuthConstants.NameHeader] = Uri.EscapeDataString(session.AuthInfo.FullName);
		response.Headers[AuthConstants.GlobalAccessHeader] = session.AuthInfo.RootRule.Access.ToString();
	}
}