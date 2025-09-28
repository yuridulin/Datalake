using Datalake.InventoryService.Application.Features.Audit.Queries.Audit;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.LogModels;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.InventoryService.Api.Controllers;

public class AuditController : ControllerBase
{
	public async Task<ActionResult<IEnumerable<LogInfo>>> GetAsync(
		[FromServices] IGetAuditQueryHandler getAuditQueryHandler,
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
		var data = await getAuditQueryHandler.HandleAsync(new(lastId, firstId, take, source, block, tag, user, group, categories, types, author), ct);

		return Ok(data);
	}
}
