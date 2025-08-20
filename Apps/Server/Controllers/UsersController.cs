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
	AuthenticationService authService,
	UsersMemoryRepository usersRepository,
	SessionManagerService sessionManager) : UsersControllerBase
{
	/// <inheritdoc />
	public override ActionResult<UserAuthInfo> AuthenticateEnergoIdUser(
		[BindRequired, FromBody] UserEnergoIdInfo energoIdInfo)
	{
		var userAuthInfo = authService.Authenticate(energoIdInfo);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.Token;

		return userAuthInfo;
	}

	/// <inheritdoc />
	public override ActionResult<UserAuthInfo> Authenticate(
		[BindRequired, FromBody] UserLoginPass loginPass)
	{
		var userAuthInfo = authService.Authenticate(loginPass);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.Token;

		return userAuthInfo;
	}

	/// <inheritdoc />
	public override ActionResult<UserAuthInfo> Identify()
	{
		var user = authenticator.Authenticate(HttpContext);

		return user;
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
	public override ActionResult<UserInfo[]> GetAll()
	{
		var user = authenticator.Authenticate(HttpContext);

		return usersRepository.GetAll(user);
	}

	/// <inheritdoc />
	public override ActionResult<UserInfo> Get(
		[BindRequired, FromRoute] Guid userGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return usersRepository.Get(user, userGuid);
	}

	/// <inheritdoc />
	public override ActionResult<UserDetailInfo> GetWithDetails(
		[BindRequired, FromRoute] Guid userGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return usersRepository.GetWithDetails(user, userGuid);
	}

	/// <inheritdoc />
	public override ActionResult<UserEnergoIdInfo[]> GetEnergoId()
	{
		var user = authenticator.Authenticate(HttpContext);

		return usersRepository.GetEnergoId(user);
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
	public override ActionResult UpdateEnergoId()
	{
		var user = authenticator.Authenticate(HttpContext);

		usersRepository.UpdateEnergoId(user);

		return NoContent();
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