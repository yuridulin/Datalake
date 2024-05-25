using DatalakeDatabase.ApiModels.Blocks;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatalakeApp.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class BlocksController(BlocksRepository blocksRepository) : ControllerBase
{
	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[BindRequired, FromBody] BlockInfo blockInfo)
	{
		return await blocksRepository.CreateAsync(blockInfo);
	}

	[HttpPost("empty")]
	public async Task<ActionResult<int>> CreateEmptyAsync()
	{
		return await blocksRepository.CreateAsync();
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
		await blocksRepository.UpdateAsync(id, block);

		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		await blocksRepository.DeleteAsync(id);

		return NoContent();
	}
}
