using Datalake.Contracts.Models.Settings;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Proxy.Controllers.Inventory;

/// <inheritdoc cref="InventorySystemControllerBase" />
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
