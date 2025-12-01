using Datalake.Contracts.Models.Sources;
using Datalake.Contracts.Requests;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Proxy.Controllers.Inventory;

/// <inheritdoc cref="InventorySourcesControllerBase" />
public class InventorySourcesController(InventoryReverseProxyService proxyService) : InventorySourcesControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<int>> CreateAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<int>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<SourceWithSettingsAndTagsInfo>> GetAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<SourceWithSettingsAndTagsInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<List<SourceWithSettingsInfo>>> GetAllAsync(
		[FromQuery] bool withCustom = false,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<SourceWithSettingsInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> UpdateAsync(
		[BindRequired, FromRoute] int sourceId,
		[BindRequired, FromBody] SourceUpdateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> DeleteAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, ct);
}
