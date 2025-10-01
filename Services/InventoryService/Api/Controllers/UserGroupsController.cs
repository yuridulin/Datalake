using Datalake.InventoryService.Application.Features.UserGroups.Commands.CreateUserGroup;
using Datalake.InventoryService.Application.Features.UserGroups.Commands.DeleteUserGroup;
using Datalake.InventoryService.Application.Features.UserGroups.Commands.MoveUserGroup;
using Datalake.InventoryService.Application.Features.UserGroups.Commands.UpdateUserGroup;
using Datalake.InventoryService.Application.Features.UserGroups.Models;
using Datalake.InventoryService.Application.Features.UserGroups.Queries.GetUserGroup;
using Datalake.InventoryService.Application.Features.UserGroups.Queries.GetUserGroups;
using Datalake.InventoryService.Application.Features.UserGroups.Queries.GetUserGroupWithDetails;
using Datalake.PrivateApi.Interfaces;
using Datalake.PublicApi.Models.UserGroups;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.InventoryService.Api.Controllers;

/// <summary>
/// Группы учетных записей
/// </summary>
[ApiController]
[Route("api/v1/user-groups")]
public class UserGroupsController(IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание новой группы пользователей
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="request">Данные запроса</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор новой группы пользователей</returns>
	[HttpPost]
	public async Task<ActionResult<Guid>> CreateAsync(
		[FromServices] ICreateUserGroupHandler handler,
		[BindRequired, FromBody] UserGroupCreateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var result = await handler.HandleAsync(new()
		{
			User = user,
			ParentGuid = request.ParentGuid,
			Name = request.Name,
			Description = request.Description,
		}, ct);

		return Ok(result);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение плоского списка групп пользователей
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список групп</returns>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<UserGroupInfo>>> GetAllAsync(
		[FromServices] IGetUserGroupsHandler handler,
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
	/// <see cref="HttpMethod.Get" />: Получение информации о выбранной группе пользователей
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Информация о группе</returns>
	[HttpGet("{userGroupGuid}")]
	public async Task<ActionResult<UserGroupInfo>> GetAsync(
		[FromServices] IGetUserGroupHandler handler,
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение иерархической структуры всех групп пользователей
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список обособленных групп с вложенными подгруппами</returns>
	[HttpGet("tree")]
	public async Task<ActionResult<UserGroupTreeInfo[]>> GetTreeAsync(
		[FromServices] IGetUserGroupsHandler handler,
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
	/// <see cref="HttpMethod.Get" />: Получение детализированной информации о группе пользователей
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Информация о группе с подгруппами и списком пользователей</returns>
	[HttpGet("{userGroupGuid}/details")]
	public async Task<ActionResult<UserGroupDetailedInfo>> GetWithDetailsAsync(
		[FromServices] IGetUserGroupWithDetailsHandler handler,
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение группы пользователей
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="request">Новые данные</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{userGroupGuid}")]
	public async Task<ActionResult> UpdateAsync(
		[FromServices] IUpdateUserGroupHandler handler,
		[BindRequired, FromRoute] Guid userGroupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
			Name = request.Name,
			Description = request.Description,
			Users = request.Users.Select(x => new UserRelationDto { Guid = x.Guid, AccessType = x.AccessType })
		}, ct);

		return NoContent();
	}

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Перемещение группы пользователей
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="parentGuid">Идентификатор новой родительской группы</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{groupGuid}/move")]
	public async Task<ActionResult> MoveAsync(
		[FromServices] IMoveUserGroupHandler handler,
		[BindRequired, FromRoute] Guid groupGuid,
		[FromQuery] Guid? parentGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new()
		{
			User = user,
			Guid = groupGuid,
			ParentGuid = parentGuid,
		}, ct);

		return NoContent();
	}

	/// <summary>
	/// <see cref="HttpMethod.Delete" />: Удаление группы пользователей
	/// </summary>
	/// <param name="handler">Обработчик</param>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{userGroupGuid}")]
	public async Task<ActionResult> DeleteAsync(
		[FromServices] IDeleteUserGroupHandler handler,
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
		}, ct);

		return NoContent();
	}
}