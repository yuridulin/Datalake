using Datalake.Contracts.Public.Models.Users;
using Datalake.Inventory.Application.Features.Users.Commands.CreateUser;
using Datalake.Inventory.Application.Features.Users.Commands.DeleteUser;
using Datalake.Inventory.Application.Features.Users.Commands.UpdateUser;
using Datalake.Inventory.Application.Features.Users.Queries.GetUsers;
using Datalake.Inventory.Application.Features.Users.Queries.GetUserWithDetails;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

/// <summary>
/// Учетные записи
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// Создание пользователя на основании переданных данных
	/// </summary>
	/// <param name="request">Данные нового пользователя</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор пользователя</returns>
	[HttpPost]
	public async Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserCreateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<ICreateUserHandler>();
		var result = await handler.HandleAsync(new()
		{
			User = user,
			Type = request.Type,
			EnergoIdGuid = request.EnergoIdGuid,
			Login = request.Login,
			Password = request.Password,
			FullName = request.FullName,
			Email = request.Email,
			AccessType = request.AccessType,
		}, ct);

		return Ok(result);
	}

	/// <summary>
	/// Получение списка пользователей
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список пользователей</returns>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<UserInfo>>> GetAllAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUsersHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// Получение детализированной информации о пользователе
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Данные о пользователе</returns>
	[HttpGet("{userGuid}")]
	public async Task<ActionResult<UserInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserWithDetailsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGuid,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// Изменение пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="request">Новые данные пользователя</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{userGuid}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGuid,
		[BindRequired, FromBody] UserUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateUserHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGuid,
			Login = request.Login,
			Password = request.Password,
			FullName = request.FullName,
			Email = request.Email,
			AccessType = request.AccessType,
		}, ct);

		return NoContent();
	}

	/// <summary>
	/// Удаление пользователя
	/// </summary>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{userGuid}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IDeleteUserHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGuid,
		}, ct);

		return NoContent();
	}
}