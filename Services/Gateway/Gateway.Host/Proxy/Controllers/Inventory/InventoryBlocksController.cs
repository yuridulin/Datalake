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
	public override Task<ActionResult<List<BlockWithTagsInfo>>> GetAllAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<BlockWithTagsInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<BlockDetailedInfo>> GetAsync(
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<BlockDetailedInfo>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<List<BlockTreeInfo>>> GetTreeAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<List<BlockTreeInfo>>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> UpdateAsync(
		[BindRequired, FromRoute] int blockId,
		[BindRequired, FromBody] BlockUpdateRequest request,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, request, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> MoveAsync(
		[BindRequired, FromRoute] int blockId,
		[FromQuery] int? parentId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<bool>> DeleteAsync(
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<bool>(HttpContext, ct);
}
