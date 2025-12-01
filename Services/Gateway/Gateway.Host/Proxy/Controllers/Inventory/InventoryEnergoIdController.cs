using Datalake.Contracts.Models.Users;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Gateway.Host.Proxy.Controllers.Inventory;

/// <inheritdoc cref="InventoryEnergoIdControllerBase" />
public class InventoryEnergoIdController(InventoryReverseProxyService proxyService) : InventoryEnergoIdControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<List<UserEnergoIdInfo>>> GetEnergoIdAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<UserEnergoIdInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> UpdateEnergoIdAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, ct);
}
