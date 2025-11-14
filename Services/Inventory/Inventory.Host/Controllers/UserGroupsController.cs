using Datalake.Contracts.Models.UserGroups;
using Datalake.Inventory.Application.Features.UserGroups.Commands.CreateUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Commands.DeleteUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Commands.MoveUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Commands.UpdateUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Models;
using Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroup;
using Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroups;
using Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroupWithDetails;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

/// <summary>
/// Группы учетных записей
/// </summary>
[ApiController]
[Route("api/user-groups")]
public class UserGroupsController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : ControllerBase
{
	/// <summary>
	/// Создание новой группы пользователей
	/// </summary>
	/// <param name="request">Данные запроса</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор новой группы пользователей</returns>
	[HttpPost]
	public async Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserGroupCreateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<ICreateUserGroupHandler>();
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
	/// Получение плоского списка групп пользователей
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список групп</returns>
	[HttpGet]
	public async Task<ActionResult<IEnumerable<UserGroupInfo>>> GetAllAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserGroupsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// Получение информации о выбранной группе пользователей
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Информация о группе</returns>
	[HttpGet("{userGroupGuid}")]
	public async Task<ActionResult<UserGroupInfo>> GetAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserGroupHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// Получение иерархической структуры всех групп пользователей
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список обособленных групп с вложенными подгруппами</returns>
	[HttpGet("tree")]
	public async Task<ActionResult<UserGroupTreeInfo[]>> GetTreeAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserGroupsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// Получение детализированной информации о группе пользователей
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Информация о группе с подгруппами и списком пользователей</returns>
	[HttpGet("{userGroupGuid}/details")]
	public async Task<ActionResult<UserGroupDetailedInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetUserGroupWithDetailsHandler>();
		var data = await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
		}, ct);

		return Ok(data);
	}

	/// <summary>
	/// Изменение группы пользователей
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="request">Новые данные</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{userGroupGuid}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateUserGroupHandler>();
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
	/// Перемещение группы пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="parentGuid">Идентификатор новой родительской группы</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{groupGuid}/move")]
	public async Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[FromQuery] Guid? parentGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IMoveUserGroupHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			Guid = groupGuid,
			ParentGuid = parentGuid,
		}, ct);

		return NoContent();
	}

	/// <summary>
	/// Удаление группы пользователей
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{userGroupGuid}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IDeleteUserGroupHandler>();
		await handler.HandleAsync(new()
		{
			User = user,
			Guid = userGroupGuid,
		}, ct);

		return NoContent();
	}
}