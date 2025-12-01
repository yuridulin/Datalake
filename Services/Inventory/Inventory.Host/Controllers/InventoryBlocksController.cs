using Datalake.Contracts.Models.Blocks;
using Datalake.Contracts.Requests;
using Datalake.Inventory.Application.Features.Blocks.Commands.CreateBlock;
using Datalake.Inventory.Application.Features.Blocks.Commands.DeleteBlock;
using Datalake.Inventory.Application.Features.Blocks.Commands.MoveBlock;
using Datalake.Inventory.Application.Features.Blocks.Commands.UpdateBlock;
using Datalake.Inventory.Application.Features.Blocks.Models;
using Datalake.Inventory.Application.Features.Blocks.Queries.GetBlockFull;
using Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksTree;
using Datalake.Inventory.Application.Features.Blocks.Queries.GetBlocksWithTags;
using Datalake.Shared.Hosting.AbstractControllers.Inventory;
using Datalake.Shared.Hosting.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Inventory.Host.Controllers;

public class InventoryBlocksController(
	IServiceProvider serviceProvider,
	IAuthenticator authenticator) : InventoryBlocksControllerBase
{
	public override async Task<ActionResult<int>> CreateAsync(
		[FromQuery] int? parentId,
		[FromBody] BlockCreateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<ICreateBlockHandler>();
		var key = await handler.HandleAsync(new()
		{
			User = user,
			ParentId = request.ParentId,
			Name = request.Name,
			Description = request.Description,
		}, ct);

		return key;
	}

	public override async Task<ActionResult<List<BlockWithTagsInfo>>> GetAllAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetBlocksWithTagsHandler>();
		var data = await handler.HandleAsync(new() { User = user }, ct);

		return data;
	}

	public override async Task<ActionResult<BlockDetailedInfo>> GetAsync(
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetBlockDetailedHandler>();
		var data = await handler.HandleAsync(new(user, blockId), ct);

		return data;
	}

	public override async Task<ActionResult<List<BlockTreeInfo>>> GetTreeAsync(
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IGetBlocksTreeHandler>();
		var data = await handler.HandleAsync(new(user), ct);

		return data;
	}

	public override async Task<ActionResult<bool>> UpdateAsync(
		[BindRequired, FromRoute] int blockId,
		[BindRequired, FromBody] BlockUpdateRequest request,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IUpdateBlockHandler>();
		var data = await handler.HandleAsync(new(
			user,
			blockId,
			request.Name,
			request.Description,
			request.Tags.Select(x => new BlockTagDto(x.Id, x.Name, x.Relation))), ct);

		return data;
	}

	public override async Task<ActionResult<bool>> MoveAsync(
		[BindRequired, FromRoute] int blockId,
		[FromQuery] int? parentId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IMoveBlockHandler>();
		var data = await handler.HandleAsync(new(user, blockId, parentId), ct);

		return data;
	}

	public override async Task<ActionResult<bool>> DeleteAsync(
		[BindRequired, FromRoute] int blockId,
		CancellationToken ct = default)
	{
		var user = authenticator.Authenticate(HttpContext);
		var handler = serviceProvider.GetRequiredService<IDeleteBlockHandler>();
		var data = await handler.HandleAsync(new(user, blockId), ct);

		return data;
	}
}