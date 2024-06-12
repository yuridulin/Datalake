using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.UserGroups;
using Datalake.Database.Repositories;
using Datalake.Server.Controllers.Base;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <summary>
/// Взаимодействие с группами пользователей
/// </summary>
/// <param name="userGroupsRepository">Репозиторий</param>
[Route("api/[controller]")]
[ApiController]
public class UserGroupsController(UserGroupsRepository userGroupsRepository) : ApiControllerBase
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

		return await userGroupsRepository.CreateAsync(user, request);
	}

	/// <summary>
	/// Получение плоского списка групп пользователей
	/// </summary>
	/// <returns>Список групп</returns>
	[HttpGet]
	public async Task<ActionResult<UserGroupInfo[]>> ReadAsync()
	{
		return await userGroupsRepository.GetInfo()
			.ToArrayAsync();
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
		return await userGroupsRepository.GetInfo()
			.Where(x => x.Guid == groupGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"группа {groupGuid}");
	}

	/// <summary>
	/// Получение иерархической структуры всех групп пользователей
	/// </summary>
	/// <returns>Список обособленных групп с вложенными подгруппами</returns>
	[HttpGet("tree")]
	public async Task<ActionResult<UserGroupTreeInfo[]>> ReadAsTreeAsync()
	{
		var tree = await userGroupsRepository.GetTreeAsync();

		return tree;
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
		return await userGroupsRepository.GetWithChildsAndUsers()
			.Where(x => x.Guid == groupGuid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"группа {groupGuid}");
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

		await userGroupsRepository.UpdateAsync(user, groupGuid, request);

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

		await userGroupsRepository.DeleteAsync(user, groupGuid);

		return NoContent();
	}
}
