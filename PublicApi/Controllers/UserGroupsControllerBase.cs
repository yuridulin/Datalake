using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.UserGroups;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.PublicApi.Controllers;

/// <summary>
/// Взаимодействие с группами пользователей
/// </summary>
[ApiController]
[Route($"{Defaults.ApiRoot}/{ControllerRoute}")]
public abstract class UserGroupsControllerBase : ControllerBase
{
	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "user-groups";

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание новой группы пользователей
	/// </summary>
	/// <param name="request">Данные запроса</param>
	/// <returns>Идентификатор новой группы пользователей</returns>
	[HttpPost]
	public abstract Task<ActionResult<UserGroupInfo>> CreateAsync(
		[BindRequired, FromBody] UserGroupCreateRequest request);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение плоского списка групп пользователей
	/// </summary>
	/// <returns>Список групп</returns>
	[HttpGet]
	public abstract Task<ActionResult<UserGroupInfo[]>> GetAllAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение информации о выбранной группе пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Информация о группе</returns>
	/// <exception cref="NotFoundException">Группа не найдена по ключу</exception>
	[HttpGet("{groupGuid}")]
	public abstract Task<ActionResult<UserGroupInfo>> GetAsync(
		[BindRequired, FromRoute] Guid groupGuid);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение иерархической структуры всех групп пользователей
	/// </summary>
	/// <returns>Список обособленных групп с вложенными подгруппами</returns>
	[HttpGet("tree")]
	public abstract Task<ActionResult<UserGroupTreeInfo[]>> GetTreeAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение детализированной информации о группе пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Информация о группе с подгруппами и списком пользователей</returns>
	/// <exception cref="NotFoundException">Группа не найдена по ключу</exception>
	[HttpGet("{groupGuid}/detailed")]
	public abstract Task<ActionResult<UserGroupDetailedInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid groupGuid);

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение группы пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="request">Новые данные</param>
	[HttpPut("{groupGuid}")]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request);

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Перемещение группы пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="parentGuid">Идентификатор новой родительской группы</param>
	[HttpPost("{groupGuid}/move")]
	public abstract Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[FromQuery] Guid? parentGuid);

	/// <summary>
	/// <see cref="HttpMethod.Delete" />: Удаление группы пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	[HttpDelete("{groupGuid}")]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid groupGuid);
}