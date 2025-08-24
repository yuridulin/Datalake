using Datalake.Database;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Users;
using Datalake.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <inheritdoc />
public class UsersController(
	DatalakeContext db,
	AuthenticationService authenticator,
	UsersMemoryRepository usersRepository,
	SessionManagerService sessionManager) : UsersControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<UserAuthInfo>> AuthenticateEnergoIdUserAsync(
		[BindRequired, FromBody] UserEnergoIdInfo energoIdInfo)
	{
		var userAuthInfo = authenticator.Authenticate(energoIdInfo);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.Token;

		return await Task.FromResult(userAuthInfo);
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserAuthInfo>> AuthenticateAsync(
		[BindRequired, FromBody] UserLoginPass loginPass)
	{
		var userAuthInfo = authenticator.Authenticate(loginPass);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.Token;

		return await Task.FromResult(userAuthInfo);
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserAuthInfo>> IdentifyAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(user);
	}

	/// <inheritdoc />
	public override async Task<ActionResult> LogoutAsync(
		[BindRequired, FromQuery] string token)
	{
		authenticator.Authenticate(HttpContext);

		sessionManager.CloseSession(token);

		return await Task.FromResult(NoContent());
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserInfo>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest userAuthRequest)
	{
		var user = authenticator.Authenticate(HttpContext);

		var info = await usersRepository.CreateAsync(db, user, userAuthRequest);

		return info;
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserInfo[]>> GetAllAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(usersRepository.GetAll(user));
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserInfo>> GetAsync(
		[BindRequired, FromRoute] Guid userGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(usersRepository.Get(user, userGuid));
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserDetailInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(usersRepository.GetWithDetails(user, userGuid));
	}

	/// <inheritdoc />
	public override async Task<ActionResult<UserEnergoIdInfo[]>> GetEnergoIdAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(usersRepository.GetEnergoId(user));
	}

	/// <inheritdoc />
	public override async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGuid,
		[BindRequired, FromBody] UserUpdateRequest userUpdateRequest)
	{
		var user = authenticator.Authenticate(HttpContext);

		await usersRepository.UpdateAsync(db, user, userGuid, userUpdateRequest);

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult> UpdateEnergoIdAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		usersRepository.UpdateEnergoId(user);

		return await Task.FromResult(NoContent());
	}

	/// <inheritdoc />
	public override async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		await usersRepository.DeleteAsync(db, user, userGuid);

		return NoContent();
	}
}