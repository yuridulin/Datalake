using Datalake.Contracts.Models.AccessRules;
using Datalake.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Shared.Hosting.Controllers.Inventory;

/// <summary>
/// Правила доступа
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Inventory")]
[Route("api/v1/inventory/access")]
public abstract class InventoryAccessControllerBase : ControllerBase
{
	/// <summary>
	/// Получение списка прямых (не глобальных) разрешений субъекта на объект
	/// </summary>
	/// <param name="userGuid">Идентификтатор пользователя</param>
	/// <param name="userGroupGuid">Идентификатор группы пользователей</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список разрешений</returns>
	[HttpGet]
	public abstract Task<ActionResult<AccessRightsInfo[]>> GetAsync(
		[FromQuery(Name = "user")] Guid? userGuid = null,
		[FromQuery(Name = "userGroup")] Guid? userGroupGuid = null,
		[FromQuery(Name = "source")] int? sourceId = null,
		[FromQuery(Name = "block")] int? blockId = null,
		[FromQuery(Name = "tag")] int? tagId = null,
		CancellationToken ct = default);

	/// <summary>
	/// Получение списка рассчитанных разрешений субъекта на объект для всех субъетов и всех объектов
	/// </summary>
	[HttpPost("calculated")]
	public abstract Task<ActionResult<IDictionary<Guid, UserAccessValue>>> GetCalculatedAccessAsync(
		[FromBody] IEnumerable<Guid>? guids,
		CancellationToken ct = default);

	/// <summary>
	/// Изменение разрешений для учетной записи
	/// </summary>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="requests">Список изменений</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("user/{userGuid}")]
	public abstract Task<ActionResult> SetUserRulesAsync(
		[FromRoute] Guid userGuid,
		[FromBody] AccessRuleForActorRequest[] requests,
		CancellationToken ct = default);

	/// <summary>
	/// Изменение разрешений для группы учетных записей
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы учетных записей</param>
	/// <param name="requests">Список изменений</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("user-group/{userGroupGuid}")]
	public abstract Task<ActionResult> SetUserGroupRulesAsync(
		[FromRoute] Guid userGroupGuid,
		[FromBody] AccessRuleForActorRequest[] requests,
		CancellationToken ct = default);

	/// <summary>
	/// Изменение разрешений на источник данных
	/// </summary>
	/// <param name="sourceId">Идентификатор источника данных</param>
	/// <param name="requests">Список изменений</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("source/{sourceId}")]
	public abstract Task<ActionResult> SetSourceRulesAsync(
		[FromRoute] int sourceId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default);

	/// <summary>
	/// Изменение разрешений для блок
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="requests">Список изменений</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("block/{blockId}")]
	public abstract Task<ActionResult> SetBlockRulesAsync(
		[FromRoute] int blockId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default);

	/// <summary>
	/// Изменение разрешений для тега
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="requests">Список изменений</param>
	/// <param name="ct">Токен отмены</param>
	[HttpPut("tag/{tagId}")]
	public abstract Task<ActionResult> SetTagRulesAsync(
		[FromRoute] int tagId,
		[FromBody] AccessRuleForObjectRequest[] requests,
		CancellationToken ct = default);
}
