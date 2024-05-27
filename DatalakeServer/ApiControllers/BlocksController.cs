using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Blocks;
using DatalakeDatabase.Repositories;
using DatalakeServer.ApiControllers.Base;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatalakeServer.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class BlocksController(BlocksRepository blocksRepository) : ApiControllerBase
{
	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[BindRequired, FromBody] BlockInfo blockInfo)
	{
		var user = Authenticate();

		return await blocksRepository.CreateAsync(user, blockInfo);
	}

	[HttpPost("empty")]
	public async Task<ActionResult<int>> CreateEmptyAsync()
	{
		var user = Authenticate();

		return await blocksRepository.CreateAsync(user);
	}

	[HttpGet]
	public async Task<ActionResult<BlockSimpleInfo[]>> ReadAsync()
	{
		return await blocksRepository.GetSimpleInfo()
			.ToArrayAsync();
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<BlockInfo>> ReadAsync(
		[BindRequired, FromRoute] int id)
	{
		return await blocksRepository.GetInfoWithAllRelations()
			.Where(x => x.Id == id)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Сущность #{id}");
	}

	[HttpGet("tree")]
	public async Task<ActionResult<BlockTreeInfo[]>> ReadAsTreeAsync()
	{
		return await blocksRepository.GetTreeAsync();
	}

	[HttpPut("{id:int}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] BlockInfo block)
	{
		var user = Authenticate();

		await blocksRepository.UpdateAsync(user, id, block);

		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = Authenticate();

		await blocksRepository.DeleteAsync(user, id);

		return NoContent();
	}
}
