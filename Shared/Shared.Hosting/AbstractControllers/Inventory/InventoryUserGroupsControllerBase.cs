using Datalake.Contracts.Models.UserGroups;
using Datalake.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Shared.Hosting.AbstractControllers.Inventory;

/// <summary>
/// Группы учетных записей
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Inventory")]
[Route("api/v1/inventory/user-groups")]
public abstract class InventoryUserGroupsControllerBase : ControllerBase
{
	/// <summary>
	/// Создание новой группы пользователей
	/// </summary>
	/// <param name="request">Данные запроса</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Идентификатор новой группы пользователей</returns>
	[HttpPost]
	public abstract Task<ActionResult<Guid>> CreateAsync(
		[BindRequired, FromBody] UserGroupCreateRequest request,
		CancellationToken ct = default);

	/// <summary>
	/// Получение плоского списка групп пользователей
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список групп</returns>
	[HttpGet]
	public abstract Task<ActionResult<IEnumerable<UserGroupInfo>>> GetAllAsync(
		CancellationToken ct = default);

	/// <summary>
	/// Получение иерархической структуры всех групп пользователей
	/// </summary>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список обособленных групп с вложенными подгруппами</returns>
	[HttpGet("tree")]
	public abstract Task<ActionResult<UserGroupTreeInfo[]>> GetTreeAsync(
		CancellationToken ct = default);

	/// <summary>
	/// Получение информации о выбранной группе пользователей
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Информация о группе</returns>
	[HttpGet("{userGroupGuid}")]
	public abstract Task<ActionResult<UserGroupInfo>> GetAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default);

	/// <summary>
	/// Получение детализированной информации о группе пользователей
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Информация о группе с подгруппами и списком пользователей</returns>
	[HttpGet("{userGroupGuid}/details")]
	public abstract Task<ActionResult<UserGroupDetailedInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default);

	/// <summary>
	/// Перемещение группы пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="parentGuid">Идентификатор новой родительской группы</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{groupGuid}/move")]
	public abstract Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[FromQuery] Guid? parentGuid,
		CancellationToken ct = default);

	/// <summary>
	/// Изменение группы пользователей
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="request">Новые данные</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("{userGroupGuid}")]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request,
		CancellationToken ct = default);

	/// <summary>
	/// Удаление группы пользователей
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы</param>
	/// <param name="ct">Токен отмены</param>
	[HttpDelete("{userGroupGuid}")]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid userGroupGuid,
		CancellationToken ct = default);
}
