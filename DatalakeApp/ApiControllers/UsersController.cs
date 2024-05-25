using DatalakeApp.Services.SessionManager;
using DatalakeDatabase.ApiModels.Users;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatalakeApp.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(
	UsersRepository usersRepository,
	SessionManagerService sessionManager) : ControllerBase
{
	[HttpPost("auth")]
	public async Task<ActionResult<UserAuthInfo>> AuthenticateAsync(
		[BindRequired, FromBody] UserLoginPass loginPass)
	{
		var userAuthInfo = await usersRepository.AuthenticateAsync(loginPass);

		var session = sessionManager.OpenSession(userAuthInfo);
		sessionManager.AddSessionToResponse(session, Response);

		userAuthInfo.Token = session.Token;

		return userAuthInfo;
	}

	[HttpPost]
	public async Task<ActionResult<bool>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest userAuthRequest)
	{
		return await usersRepository.CreateAsync(userAuthRequest);
	}

	[HttpGet]
	public async Task<ActionResult<UserInfo[]>> ReadAsync()
	{
		return await usersRepository.GetInfo()
			.ToArrayAsync();
	}

	[HttpGet("{name}")]
	public async Task<ActionResult<UserInfo>> ReadAsync(
		[BindRequired, FromRoute] string name)
	{
		return await usersRepository.GetInfo()
			.Where(x => x.LoginName == name)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Учётная запись {name}");
	}

	[HttpGet("{name}/detailed")]
	public async Task<ActionResult<UserDetailInfo>> ReadWithDetailsAsync(
		[BindRequired, FromRoute] string name)
	{
		return await usersRepository.GetDetailInfo()
			.Where(x => x.LoginName == name)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Учётная запись {name}");
	}

	[HttpPut("{name}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] string name,
		[BindRequired, FromBody] UserUpdateRequest userUpdateRequest)
	{
		await usersRepository.UpdateAsync(name, userUpdateRequest);

		return NoContent();
	}

	[HttpDelete("{name}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] string name)
	{
		await usersRepository.DeleteAsync(name);

		return NoContent();
	}
}
