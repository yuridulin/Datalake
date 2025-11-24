using Datalake.Contracts.Models.LogModels;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Features.Audit.Queries.GetAudit;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Inventory.Host.Controllers;

public class InventoryAuditController(
	IServiceProvider serviceProvider) : InventoryAuditControllerBase
{
	public override async Task<ActionResult<IEnumerable<LogInfo>>> GetAsync(
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
		CancellationToken ct = default)
	{
		var handler = serviceProvider.GetRequiredService<IGetAuditHandler>();
		var data = await handler.HandleAsync(new(lastId, firstId, take, source, block, tag, user, group, categories, types, author), ct);

		return Ok(data);
	}
}
