using Datalake.Contracts.Models.Sources;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.ProxyControllers.Inventory;

/// <inheritdoc />
public class InventorySourcesController(InventoryReverseProxyService proxyService) : InventorySourcesControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<int>> CreateAsync(
		[FromBody] SourceInfo? request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<int>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<SourceInfo>> GetAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<SourceInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<IEnumerable<SourceInfo>>> GetAllAsync(
		[FromQuery] bool withCustom = false,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<IEnumerable<SourceInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int sourceId,
		[BindRequired, FromBody] SourceUpdateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int sourceId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, ct);
}
