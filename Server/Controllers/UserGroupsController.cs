using Datalake.Database;
using Datalake.Database.Exceptions;
using Datalake.Database.Models.UserGroups;
using Datalake.Server.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с группами пользователей
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UserGroupsController(DatalakeContext db) : ApiControllerBase
{
	/// <summary>
	/// Создание новой группы пользователей
	/// </summary>
	/// <param name="request">Данные запроса</param>
	/// <returns>Идентификатор новой группы пользователей</returns>
	[HttpPost]
	public async Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserGroupCreateRequest request)
	{
		var user = Authenticate();

		return await db.UserGroupsRepository.CreateAsync(user, request);
	}

	/// <summary>
	/// Получение плоского списка групп пользователей
	/// </summary>
	/// <returns>Список групп</returns>
	[HttpGet]
	public async Task<ActionResult<UserGroupInfo[]>> ReadAllAsync()
	{
		var user = Authenticate();

		return await db.UserGroupsRepository.ReadAllAsync(user);
	}

	/// <summary>
	/// Получение информации о выбранной группе пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Информация о группе</returns>
	/// <exception cref="NotFoundException">Группа не найдена по ключу</exception>
	[HttpGet("{groupGuid}")]
	public async Task<ActionResult<UserGroupInfo>> ReadAsync(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		var user = Authenticate();

		return await db.UserGroupsRepository.ReadAsync(user, groupGuid);
	}

	/// <summary>
	/// Получение иерархической структуры всех групп пользователей
	/// </summary>
	/// <returns>Список обособленных групп с вложенными подгруппами</returns>
	[HttpGet("tree")]
	public async Task<ActionResult<UserGroupTreeInfo[]>> ReadAsTreeAsync()
	{
		var user = Authenticate();

		return await db.UserGroupsRepository.ReadAllAsTreeAsync(user);
	}

	/// <summary>
	/// Получение детализированной информации о группе пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Информация о группе с подгруппами и списком пользователей</returns>
	/// <exception cref="NotFoundException">Группа не найдена по ключу</exception>
	[HttpGet("{groupGuid}/detailed")]
	public async Task<ActionResult<UserGroupDetailedInfo>> ReadWithDetailsAsync(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		var user = Authenticate();

		return await db.UserGroupsRepository.ReadWithDetailsAsync(user, groupGuid);
	}

	/// <summary>
	/// Изменение группы пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="request">Новые данные</param>
	[HttpPut("{groupGuid}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request)
	{
		var user = Authenticate();

		await db.UserGroupsRepository.UpdateAsync(user, groupGuid, request);

		return NoContent();
	}

	/// <summary>
	/// Перемещение группы пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="parentGuid">Идентификатор новой родительской группы</param>
	[HttpPost("{groupGuid}/move")]
	public async Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[FromQuery] Guid? parentGuid)
	{
		var user = Authenticate();

		await db.UserGroupsRepository.MoveAsync(user, groupGuid, parentGuid);

		return NoContent();
	}

	/// <summary>
	/// Удаление группы пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	[HttpDelete("{groupGuid}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		var user = Authenticate();

		await db.UserGroupsRepository.DeleteAsync(user, groupGuid);

		return NoContent();
	}
}
