using Datalake.Contracts.Models.Blocks;
using Datalake.Gateway.Host.Services;
using Datalake.Shared.Hosting.Controllers.Inventory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Gateway.Host.ProxyControllers.Inventory;

/// <inheritdoc />
public class InventoryBlocksController(InventoryReverseProxyService proxyService) : InventoryBlocksControllerBase
{
	/// <inheritdoc />
	public override Task<ActionResult<int>> CreateAsync(
		[FromQuery] int? parentId,
		[FromBody] BlockFullInfo? blockInfo,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<int>(HttpContext, blockInfo, ct);

	/// <inheritdoc />
	public override Task<ActionResult<BlockWithTagsInfo[]>> GetAllAsync(
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<BlockWithTagsInfo[]>(HttpContext, ct);

	/// <inheritdoc />
	public override Task<ActionResult<BlockFullInfo>> GetAsync(
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
			=> proxyService.ProxyAsync<BlockFullInfo>(HttpContext, ct);

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
