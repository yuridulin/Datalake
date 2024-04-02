using DatalakeDatabase.ApiModels.Blocks;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeApp.ApiControllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class BlocksController(BlocksRepository blocksRepository) : ControllerBase
	{
		[HttpPost]
		public async Task<ActionResult<int>> CreateAsync(
			[FromBody] BlockInfo blockInfo)
		{
			return await blocksRepository.CreateAsync(blockInfo);
		}

		[HttpGet]
		public async Task<ActionResult<BlockSimpleInfo[]>> ReadAsync()
		{
			return await blocksRepository.GetBlocksSimpleInfo()
				.ToArrayAsync();
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<BlockInfo>> ReadAsync(
			[FromRoute] int id)
		{
			return await blocksRepository.GetBlocksWithAllRelations()
				.Where(x => x.Id == id)
				.FirstOrDefaultAsync()
				?? throw new NotFoundException($"Сущность #{id}");
		}

		[HttpGet("tree")]
		public async Task<ActionResult<BlockTreeInfo[]>> ReadAsTreeAsync()
		{
			return await blocksRepository.GetBlocksAsTreeAsync();
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult> UpdateAsync(
			[FromRoute] int id,
			[FromBody] BlockInfo block)
		{
			await blocksRepository.UpdateAsync(id, block);

			return NoContent();
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult> DeleteAsync(
			[FromRoute] int id)
		{
			await blocksRepository.DeleteAsync(id);

			return NoContent();
		}
	}
}
