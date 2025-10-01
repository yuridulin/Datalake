using Datalake.InventoryService.Application.Features.Users.Commands.CreateUser;
using Datalake.InventoryService.Application.Features.Users.Commands.DeleteUser;
using Datalake.InventoryService.Application.Features.Users.Commands.UpdateUser;
using Datalake.InventoryService.Application.Features.Users.Queries.GetUsers;
using Datalake.InventoryService.Application.Features.Users.Queries.GetUserWithDetails;
using Datalake.PrivateApi.Interfaces;
using Datalake.PublicApi.Models.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.InventoryService.Api.Controllers;

/// <summary>
/// Учетные записи
/// </summary>
[ApiController]
[Route("api/v1/users")]
public class UsersController(IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание пользователя на основании переданных данных
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="request">Данные нового пользователя</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор пользователя</returns>
	[HttpPost]
	public async Task<ActionResult<Guid>> CreateAsync(
		[FromServices] ICreateUserHandler handler,
		[BindRequired, FromBody] UserCreateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var result = await handler.HandleAsync(new()
		{
			User = user,
			AccessType = request.AccessType,
			Type = request.Type,
			EnergoIdGuid = request.EnergoIdGuid,
			FullName = request.FullName,
			Login = request.Login,
			Password = request.Password,
			StaticHost = request.StaticHost,
		}, ct);

		return Ok(result);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение списка пользователей
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список пользователей</returns>
	public async Task<ActionResult<IEnumerable<UserInfo>>> GetAllAsync(
		[FromServices] IGetUsersHandler handler,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new()
		{
			User = user,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение детализированной информации о пользователе
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Данные о пользователе</returns>
	public async Task<ActionResult<UserDetailInfo>> GetWithDetailsAsync(
		[FromServices] IGetUserWithDetailsHandler handler,
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGuid,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение пользователя
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="request">Новые данные пользователя</param>
	/// <param name="ct">Токен отмены</param>
	public async Task<ActionResult> UpdateAsync(
		[FromServices] IUpdateUserHandler handler,
		[BindRequired, FromRoute] Guid userGuid,
		[BindRequired, FromBody] UserUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGuid,
			AccessType = request.AccessType,
			Type = request.Type,
			EnergoIdGuid = request.EnergoIdGuid,
			FullName = request.FullName,
			Login = request.Login,
			Password = request.Password,
			StaticHost = request.StaticHost,
			GenerateNewHash = request.CreateNewStaticHash
		}, ct);

		return NoContent();
	}

	/// <summary>
	/// <see cref="HttpMethod.Delete" />: Удаление пользователя
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="ct">Токен отмены</param>
	public async Task<ActionResult> DeleteAsync(
		[FromServices] IDeleteUserHandler handler,
		[BindRequired, FromRoute] Guid userGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGuid,
		}, ct);

		return NoContent();
	}
}