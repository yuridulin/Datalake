using Datalake.Database;
using Datalake.Database.InMemory.Repositories;
using Datalake.PublicApi.Controllers;
using Datalake.PublicApi.Models.Blocks;
using Datalake.Server.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Threading.Tasks;

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
	public override async Task<ActionResult<BlockWithTagsInfo[]>> GetAllAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(blocksRepository.GetAll(user));
	}

	/// <inheritdoc />
	public override async Task<ActionResult<BlockFullInfo>> GetAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(blocksRepository.Get(user, id));
	}

	/// <inheritdoc />
	public override async Task<ActionResult<BlockTreeInfo[]>> GetTreeAsync()
	{
		var user = authenticator.Authenticate(HttpContext);

		return await Task.FromResult(blocksRepository.GetAllAsTree(user));
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