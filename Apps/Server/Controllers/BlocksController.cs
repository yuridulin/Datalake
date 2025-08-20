using Datalake.Database;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.Blocks;
using Datalake.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Datalake.Server.Controllers;

/// <inheritdoc />
public class BlocksController(
	DatalakeContext db,
	AuthenticationService authenticator,
	BlocksMemoryRepository blocksRepository) : BlocksControllerBase
{
	/// <inheritdoc />
	public override async Task<ActionResult<BlockWithTagsInfo>> CreateAsync(
		[BindRequired, FromBody] BlockFullInfo blockInfo)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await blocksRepository.CreateAsync(db, user, blockInfo: blockInfo);
	}

	/// <inheritdoc />
	public override async Task<ActionResult<BlockWithTagsInfo>> CreateEmptyAsync(
		[FromQuery] int? parentId)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await blocksRepository.CreateAsync(db, user, parentId: parentId);
	}

	/// <inheritdoc />
	public override ActionResult<BlockWithTagsInfo[]> GetAll()
	{
		var user = authenticator.Authenticate(HttpContext);

		return blocksRepository.GetAll(user);
	}

	/// <inheritdoc />
	public override ActionResult<BlockFullInfo> Get(
		[BindRequired, FromRoute] int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		return blocksRepository.Get(user, id);
	}

	/// <inheritdoc />
	public override ActionResult<BlockTreeInfo[]> GetTree()
	{
		var user = authenticator.Authenticate(HttpContext);

		return blocksRepository.GetAllAsTree(user);
	}

	/// <inheritdoc />
	public override async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] BlockUpdateRequest block)
	{
		var user = authenticator.Authenticate(HttpContext);

		await blocksRepository.UpdateAsync(db, user, id, block);

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult> MoveAsync(
		[BindRequired, FromRoute] int id,
		[FromQuery] int? parentId)
	{
		var user = authenticator.Authenticate(HttpContext);

		await blocksRepository.MoveAsync(db, user, id, parentId);

		return NoContent();
	}

	/// <inheritdoc />
	public override async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		await blocksRepository.DeleteAsync(db, user, id);

		return NoContent();
	}
}