using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Models.LogModels;
using Datalake.Inventory.Application.Features.Audit.Queries.GetAudit;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Inventory.Host.Controllers;

/// <summary>
/// Аудит изменений
/// </summary>
[ApiController]
[Route("api/audit")]
public class AuditController(
	IServiceProvider serviceProvider) : ControllerBase
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
	public async Task<ActionResult<IEnumerable<LogInfo>>> GetAsync(
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
		var handler = serviceProvider.GetRequiredService<IGetAuditHandler>();
		var data = await handler.HandleAsync(new(lastId, firstId, take, source, block, tag, user, group, categories, types, author), ct);

		return Ok(data);
	}
}
