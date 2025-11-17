using Datalake.Contracts.Models.Users;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Microsoft.AspNetCore.Mvc;

namespace Datalake.Gateway.Host.ProxyControllers.Inventory;

/// <inheritdoc cref="InventoryEnergoIdControllerBase" />
public class InventoryEnergoIdController(InventoryReverseProxyService proxyService) : InventoryEnergoIdControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<UserEnergoIdInfo[]>> GetEnergoIdAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<UserEnergoIdInfo[]>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult> UpdateEnergoIdAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, ct);
}
