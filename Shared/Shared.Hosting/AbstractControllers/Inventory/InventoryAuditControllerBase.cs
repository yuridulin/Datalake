using Datalake.Contracts.Models.LogModels;
using Datalake.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Shared.Hosting.AbstractControllers.Inventory;

/// <summary>
/// Аудит изменений
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "Inventory")]
[Route("api/v1/inventory/audit")]
public abstract class InventoryAuditControllerBase : ControllerBase
{
	/// <summary>
	/// Получение списка сообщений аудита
	/// </summary>
	/// <param name="lastId">Идентификатор последнего сообщения. Будут присланы только более поздние</param>
	/// <param name="firstId">Идентификатор первого сообщения. Будут присланы только более ранние</param>
	/// <param name="take"></param>
	/// <param name="source"></param>
	/// <param name="block"></param>
	/// <param name="tag"></param>
	/// <param name="user"></param>
	/// <param name="group"></param>
	/// <param name="categories"></param>
	/// <param name="types"></param>
	/// <param name="author"></param>
	/// <param name="ct">Токен отмены</param>
	/// <returns>Список сообщений аудита</returns>
	[HttpGet]
	public abstract Task<ActionResult<IEnumerable<LogInfo>>> GetAsync(
		[FromQuery] int? lastId = null,
		[FromQuery] int? firstId = null,
		[FromQuery] int? take = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] int? tag = null,
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? group = null,
		[FromQuery(Name = nameof(categories) + "[]")] LogCategory[]? categories = null,
		[FromQuery(Name = nameof(types) + "[]")] LogType[]? types = null,
		[FromQuery] Guid? author = null,
		CancellationToken ct = default);
}
