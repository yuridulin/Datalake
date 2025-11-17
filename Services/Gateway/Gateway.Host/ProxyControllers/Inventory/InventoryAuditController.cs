using Datalake.Contracts.Models.LogModels;
using Datalake.Domain.Enums;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Gateway.Host.ProxyControllers.Inventory;

/// <inheritdoc cref="InventoryAuditControllerBase" />
public class InventoryAuditController(InventoryReverseProxyService proxyService) : InventoryAuditControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<IEnumerable<LogInfo>>> GetAsync(
		[FromQuery] int? lastId = null,
		[FromQuery] int? firstId = null,
		[FromQuery] int? take = null,
		[FromQuery] int? source = null,
		[FromQuery] int? block = null,
		[FromQuery] Guid? tag = null,
		[FromQuery] Guid? user = null,
		[FromQuery] Guid? group = null,
		[FromQuery(Name = "categories[]")] LogCategory[]? categories = null,
		[FromQuery(Name = "types[]")] LogType[]? types = null,
		[FromQuery] Guid? author = null,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<IEnumerable<LogInfo>>(HttpContext, ct);
}
