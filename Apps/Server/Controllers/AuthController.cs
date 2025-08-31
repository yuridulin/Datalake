using Datalake.Database.InMemory.Stores.Derived;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Users;
using Datalake.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <inheritdoc />
public class AuthController(
	AuthenticationService authenticator,
	DatalakeSessionsStore sessionsStore) : AuthControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<UserSessionInfo>> AuthenticateLocalAsync(
		[BindRequired, FromBody] UserLoginPass loginPass)
	{
		var userAuthInfo = authenticator.Authenticate(loginPass);

		var session = await sessionsStore.OpenSessionAsync(userAuthInfo, UserType.Local);
		AddSessionToResponse(session, Response);

		return await Task.FromResult(session);
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserSessionInfo>> AuthenticateEnergoIdUserAsync(
		[BindRequired, FromBody] UserEnergoIdInfo energoIdInfo)
	{
		var userAuthInfo = authenticator.Authenticate(energoIdInfo);

		var session = await sessionsStore.OpenSessionAsync(userAuthInfo, UserType.EnergoId);
		AddSessionToResponse(session, Response);

		return await Task.FromResult(session);
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserSessionInfo?>> IdentifyAsync()
	{
		authenticator.Authenticate(HttpContext);
		var session = await sessionsStore.GetExistSessionAsync(HttpContext);
		if (session != null)
		{
			AddSessionToResponse(session, Response);
		}
		return session;
	}

	/// <inheritdoc />
	public override async Task<ActionResult> LogoutAsync(
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