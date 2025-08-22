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
	#region Константы путей

	/// <summary>
	/// Основной путь к контроллеру
	/// </summary>
	public const string ControllerRoute = "user-groups";

	/// <inheritdoc cref="CreateAsync(UserGroupCreateRequest)" />
	public const string Create = "";

	/// <inheritdoc cref="GetAllAsync()" />
	public const string GetAll = "";

	/// <inheritdoc cref="GetAsync(Guid)" />
	public const string Get = "{groupGuid}";

	/// <inheritdoc cref="GetTreeAsync()" />
	public const string GetTree = "tree";

	/// <inheritdoc cref="GetWithDetailsAsync(Guid)" />
	public const string GetWithDetails = "{groupGuid}/detailed";

	/// <inheritdoc cref="UpdateAsync(Guid, UserGroupUpdateRequest)" />
	public const string Update = "{groupGuid}";

	/// <inheritdoc cref="MoveAsync(Guid, Guid?)" />
	public const string Move = "{groupGuid}/move";

	/// <inheritdoc cref="DeleteAsync(Guid)" />
	public const string Delete = "{groupGuid}";

	#endregion Константы путей

	#region Методы

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Создание новой группы пользователей
	/// </summary>
	/// <param name="request">Данные запроса</param>
	/// <returns>Идентификатор новой группы пользователей</returns>
	[HttpPost(Create)]
	public abstract Task<ActionResult<UserGroupInfo>> CreateAsync(
		[BindRequired, FromBody] UserGroupCreateRequest request);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение плоского списка групп пользователей
	/// </summary>
	/// <returns>Список групп</returns>
	[HttpGet(GetAll)]
	public abstract Task<ActionResult<UserGroupInfo[]>> GetAllAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение информации о выбранной группе пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Информация о группе</returns>
	/// <exception cref="NotFoundException">Группа не найдена по ключу</exception>
	[HttpGet(Get)]
	public abstract Task<ActionResult<UserGroupInfo>> GetAsync(
		[BindRequired, FromRoute] Guid groupGuid);

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение иерархической структуры всех групп пользователей
	/// </summary>
	/// <returns>Список обособленных групп с вложенными подгруппами</returns>
	[HttpGet(GetTree)]
	public abstract Task<ActionResult<UserGroupTreeInfo[]>> GetTreeAsync();

	/// <summary>
	/// <see cref="HttpMethod.Get" />: Получение детализированной информации о группе пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Информация о группе с подгруппами и списком пользователей</returns>
	/// <exception cref="NotFoundException">Группа не найдена по ключу</exception>
	[HttpGet(GetWithDetails)]
	public abstract Task<ActionResult<UserGroupDetailedInfo>> GetWithDetailsAsync(
		[BindRequired, FromRoute] Guid groupGuid);

	/// <summary>
	/// <see cref="HttpMethod.Put" />: Изменение группы пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="request">Новые данные</param>
	[HttpPut(Update)]
	public abstract Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[BindRequired, FromBody] UserGroupUpdateRequest request);

	/// <summary>
	/// <see cref="HttpMethod.Post" />: Перемещение группы пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="parentGuid">Идентификатор новой родительской группы</param>
	[HttpPost(Move)]
	public abstract Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] Guid groupGuid,
		[FromQuery] Guid? parentGuid);

	/// <summary>
	/// <see cref="HttpMethod.Delete" />: Удаление группы пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	[HttpDelete(Delete)]
	public abstract Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] Guid groupGuid);

	#endregion Методы
}
