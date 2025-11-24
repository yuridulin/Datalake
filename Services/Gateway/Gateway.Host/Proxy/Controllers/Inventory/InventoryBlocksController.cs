using Datalake.Contracts.Models.Blocks;
using Datalake.Contracts.Requests;
using Datalake.Gateway.Host.Proxy.Services;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.Proxy.Controllers.Inventory;

/// <inheritdoc cref="InventoryBlocksControllerBase" />
public class InventoryBlocksController(InventoryReverseProxyService proxyService) : InventoryBlocksControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<int>> CreateAsync(
		[FromQuery] int? parentId,
		[FromBody, BindRequired] BlockCreateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<int>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<BlockWithTagsInfo[]>> GetAllAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<BlockWithTagsInfo[]>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<BlockDetailedInfo>> GetAsync(
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<BlockDetailedInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<BlockTreeInfo[]>> GetTreeAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<BlockTreeInfo[]>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int blockId,
		[BindRequired, FromBody] BlockUpdateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] int blockId,
		[FromQuery] int? parentId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync(HttpContext, ct);
}
