using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Features.Audit.Queries.GetAudit;
using Datalake.Inventory.Api.Models.LogModels;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Inventory.App.Controllers;

/// <summary>
/// Аудит изменений
/// </summary>
[ApiController]
[Route("api/v1/blocks")]
public class AuditController : ControllerBase
{
	/// <summary>
	/// Получение списка сообщений аудита
	/// </summary>
	/// <param name="handler">Обработчик</param>
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
	public async Task<ActionResult<IEnumerable<LogInfo>>> GetAsync(
		[FromServices] IGetAuditHandler handler,
		[FromQuery] int? lastId = null,
		[FromQuery] int? firstId = null,
		[FromQuery] int? take = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] Guid? tag = null,
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? group = null,
		[FromQuery(Name = nameof(categories) + "[]")] LogCategory[]? categories = null,
		[FromQuery(Name = nameof(types) + "[]")] LogType[]? types = null,
		[FromQuery] Guid? author = null,
		CancellationToken ct = default)
	{
		var data = await handler.HandleAsync(new(lastId, firstId, take, source, block, tag, user, group, categories, types, author), ct);

		return Ok(data);
	}
}
