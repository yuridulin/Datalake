using Datalake.Contracts.Models.Settings;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.ProxyControllers.Inventory;

/// <inheritdoc />
public class InventorySystemController(InventoryReverseProxyService proxyService) : InventorySystemControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<SettingsInfo>> GetSettingsAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<SettingsInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult> UpdateSettingsAsync(
		[BindRequired][FromBody] SettingsInfo newSettings,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, newSettings, ct);

	/// <inheritdoc />
	public override Task<ActionResult> RestartStateAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, ct);
}
