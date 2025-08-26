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
	SessionManagerService sessionManager) : AuthControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<UserSessionInfo>> AuthenticateLocalAsync(
		[BindRequired, FromBody] UserLoginPass loginPass)
	{
		var userAuthInfo = authenticator.Authenticate(loginPass);

		var session = sessionManager.OpenSession(userAuthInfo, UserType.Local);
		sessionManager.AddSessionToResponse(session, Response);

		return await Task.FromResult(session);
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserSessionInfo>> AuthenticateEnergoIdUserAsync(
		[BindRequired, FromBody] UserEnergoIdInfo energoIdInfo)
	{
		var userAuthInfo = authenticator.Authenticate(energoIdInfo);

		var session = sessionManager.OpenSession(userAuthInfo, UserType.EnergoId);
		sessionManager.AddSessionToResponse(session, Response);

		return await Task.FromResult(session);
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserSessionInfo?>> IdentifyAsync()
	{
		authenticator.Authenticate(HttpContext);
		var session = sessionManager.GetExistSession(HttpContext);
		return await Task.FromResult(session);
	}

	/// <inheritdoc />
	public override async Task<ActionResult> LogoutAsync(
		[BindRequired, FromQuery] string token)
	{
		authenticator.Authenticate(HttpContext);

		sessionManager.CloseSession(token);

		return await Task.FromResult(NoContent());
	}
}