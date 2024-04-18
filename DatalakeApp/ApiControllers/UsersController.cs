using DatalakeDatabase.ApiModels.Users;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatalakeApp.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(UsersRepository usersRepository) : ControllerBase
{
	[HttpPost]
	public async Task<ActionResult<UserAuthInfo>> AuthenticateAsync(
		[BindRequired, FromBody] UserLoginPass loginPass)
	{
		return await usersRepository.AuthenticateAsync(loginPass);
	}

	[HttpPost]
	public async Task<ActionResult<bool>> CreateAsync(
		[BindRequired, FromBody] UserAuthRequest userAuthRequest)
	{
		return await usersRepository.CreateAsync(userAuthRequest);
	}

	[HttpGet]
	public async Task<ActionResult<UserInfo[]>> ReadAsync()
	{
		return await usersRepository.GetUsers()
			.ToArrayAsync();
	}

	[HttpGet("{name:string}")]
	public async Task<ActionResult<UserInfo>> ReadAsync(
		[BindRequired, FromRoute] string name)
	{
		return await usersRepository.GetUsers()
			.Where(x => x.LoginName == name)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Учётная запись {name}");
	}

	[HttpPut("{name:string}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] string name,
		[BindRequired, FromBody] UserUpdateRequest userUpdateRequest)
	{
		await usersRepository.UpdateAsync(name, userUpdateRequest);

		return NoContent();
	}

	[HttpDelete("{name:string}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] string name)
	{
		await usersRepository.DeleteAsync(name);

		return NoContent();
	}
}
