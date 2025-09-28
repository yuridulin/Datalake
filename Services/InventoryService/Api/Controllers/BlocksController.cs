using Datalake.InventoryService.Api.Services;
using Datalake.InventoryService.Application.Features.Blocks.Commands.CreateBlock;
using Datalake.InventoryService.Application.Features.Blocks.Commands.DeleteBlock;
using Datalake.InventoryService.Application.Features.Blocks.Commands.MoveBlock;
using Datalake.InventoryService.Application.Features.Blocks.Commands.UpdateBlock;
using Datalake.InventoryService.Application.Features.Blocks.Queries.BlockFull;
using Datalake.InventoryService.Application.Features.Blocks.Queries.BlocksTree;
using Datalake.InventoryService.Application.Features.Blocks.Queries.BlocksWithTags;
using Datalake.PublicApi.Models.Blocks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.InventoryService.Api.Controllers;

/// <inheritdoc />
public class BlocksController(AuthenticationService authenticator) : ControllerBase
{
	public async Task<ActionResult<int>> CreateAsync(
		[FromServices] ICreateBlockHandler createBlockHandler,
		[BindRequired, FromBody] BlockFullInfo blockInfo,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var key = await createBlockHandler.HandleAsync(new(user, blockInfo.ParentId, blockInfo.Name, blockInfo.Description), ct);
		return key;
	}

	public async Task<ActionResult<int>> CreateEmptyAsync(
		[FromServices] ICreateBlockHandler createBlockHandler,
		[FromQuery] int? parentId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var key = await createBlockHandler.HandleAsync(new(user, parentId, null, null), ct);
		return key;
	}

	/// <inheritdoc />
	public async Task<ActionResult<BlockWithTagsInfo[]>> GetAllAsync(
		[FromServices] GetBlocksWithTagsQueryHandler query,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await query.HandleAsync(new(user), ct);

		return Ok(data);
	}

	/// <inheritdoc />
	public async Task<ActionResult<BlockFullInfo>> GetAsync(
		[FromServices] GetBlockFullQueryHandler query,
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await query.HandleAsync(new(user, blockId), ct);

		return Ok(data);
	}

	/// <inheritdoc />
	public async Task<ActionResult<BlockTreeInfo[]>> GetTreeAsync(
		[FromServices] GetBlockTreeQueryHandler query,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		var data = await query.HandleAsync(new(user), ct);

		return Ok(data);
	}

	/// <inheritdoc />
	public async Task<ActionResult> UpdateAsync(
		[FromServices] IUpdateBlockHandler updateBlockHandler,
		[BindRequired, FromRoute] int blockId,
		[BindRequired, FromBody] BlockUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await updateBlockHandler.HandleAsync(new(user, blockId, request.Name, request.Description), ct);

		return NoContent();
	}

	/// <inheritdoc />
	public async Task<ActionResult> MoveAsync(
		[FromServices] IMoveBlockHandler moveBlockHandler,
		[BindRequired, FromRoute] int blockId,
		[FromQuery] int? parentId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await moveBlockHandler.HandleAsync(new(user, blockId, parentId), ct);

		return NoContent();
	}

	/// <inheritdoc />
	public async Task<ActionResult> DeleteAsync(
		[FromServices] IDeleteBlockHandler deleteBlockHandler,
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);

		await deleteBlockHandler.HandleAsync(new(user, blockId), ct);

		return NoContent();
	}
}