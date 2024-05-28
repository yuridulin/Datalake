using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Users;
using DatalakeServer.Services.SessionManager;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using DatalakeServer.ApiControllers.Base;

namespace DatalakeServer.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(
	UsersRepository usersRepository,
	SessionManagerService sessionManager) : ApiControllerBase
{
	[HttpPost("auth")]
	public async Task<ActionResult<UserAuthInfo>> AuthenticateAsync(
		[BindRequired, FromBody] UserLoginPass loginPass)
	{
		var userAuthInfo = await usersRepository.AuthenticateAsync(loginPass);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.User.Token;

		return userAuthInfo;
	}

	[HttpPost]
	public async Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest userAuthRequest)
	{
		var user = Authenticate();

		return await usersRepository.CreateAsync(user, userAuthRequest);
	}

	[HttpGet]
	public async Task<ActionResult<UserInfo[]>> ReadAsync()
	{
		return await usersRepository.GetInfo()
			.ToArrayAsync();
	}

	[HttpGet("{userGuid}")]
	public async Task<ActionResult<UserInfo>> ReadAsync(
		[BindRequired, FromRoute] Guid userGuid)
	{
		return await usersRepository.GetInfo()
			.Where(x => x.UserGuid == userGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Учётная запись {userGuid}");
	}

	[HttpGet("{userGuid}/detailed")]
	public async Task<ActionResult<UserDetailInfo>> ReadWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGuid)
	{
		return await usersRepository.GetDetailInfo()
			.Where(x => x.UserGuid == userGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Учётная запись {userGuid}");
	}

	[HttpPut("{userGuid}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGuid,
		[BindRequired, FromBody] UserUpdateRequest userUpdateRequest)
	{
		var user = Authenticate();

		await usersRepository.UpdateAsync(user, userGuid, userUpdateRequest);

		return NoContent();
	}

	[HttpDelete("{userGuid}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGuid)
	{
		var user = Authenticate();

		await usersRepository.DeleteAsync(user, userGuid);

		return NoContent();
	}
}
