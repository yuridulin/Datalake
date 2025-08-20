using Datalake.Database;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.UserGroups;
using Datalake.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с группами пользователей
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UserGroupsController(
	DatalakeContext db,
	AuthenticationService authenticator,
	UserGroupsMemoryRepository userGroupsRepository) : ControllerBase
{
	/// <summary>
	/// Создание новой группы пользователей
	/// </summary>
	/// <param name="request">Данные запроса</param>
	/// <returns>Идентификатор новой группы пользователей</returns>
	[HttpPost]
	public async Task<ActionResult<UserGroupInfo>> CreateAsync(
		[BindRequired, FromBody] UserGroupCreateRequest request)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await userGroupsRepository.CreateAsync(db, user, request);
	}

	/// <summary>
	/// Получение плоского списка групп пользователей
	/// </summary>
	/// <returns>Список групп</returns>
	[HttpGet]
	public ActionResult<UserGroupInfo[]> ReadAll()
	{
		var user = authenticator.Authenticate(HttpContext);

		return userGroupsRepository.ReadAll(user);
	}

	/// <summary>
	/// Получение информации о выбранной группе пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Информация о группе</returns>
	/// <exception cref="NotFoundException">Группа не найдена по ключу</exception>
	[HttpGet("{groupGuid}")]
	public ActionResult<UserGroupInfo> Read(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return userGroupsRepository.Read(user, groupGuid);
	}

	/// <summary>
	/// Получение иерархической структуры всех групп пользователей
	/// </summary>
	/// <returns>Список обособленных групп с вложенными подгруппами</returns>
	[HttpGet("tree")]
	public ActionResult<UserGroupTreeInfo[]> ReadAsTree()
	{
		var user = authenticator.Authenticate(HttpContext);

		return userGroupsRepository.ReadAllAsTree(user);
	}

	/// <summary>
	/// Получение детализированной информации о группе пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Информация о группе с подгруппами и списком пользователей</returns>
	/// <exception cref="NotFoundException">Группа не найдена по ключу</exception>
	[HttpGet("{groupGuid}/detailed")]
	public ActionResult<UserGroupDetailedInfo> ReadWithDetails(
		[BindRequired, FromRoute] Guid groupGuid)
	{
		var user = authenticator.Authenticate(HttpContext);

		return userGroupsRepository.ReadWithDetails(user, groupGuid);
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
		var user = authenticator.Authenticate(HttpContext);

		await userGroupsRepository.UpdateAsync(db, user, groupGuid, request);

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
		var user = authenticator.Authenticate(HttpContext);

		await userGroupsRepository.MoveAsync(db, user, groupGuid, parentGuid);

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
		var user = authenticator.Authenticate(HttpContext);

		await userGroupsRepository.DeleteAsync(db, user, groupGuid);

		return NoContent();
	}
}
