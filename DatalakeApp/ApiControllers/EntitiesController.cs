using DatalakeDatabase.ApiModels.Blocks;
using DatalakeDatabase.Models;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeApp.ApiControllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class EntitiesController(BlocksRepository blocksRepository) : ControllerBase
	{
		[HttpPost]
		public async Task<ActionResult<int>> CreateEntityAsync(
			[FromBody] BlockInfo blockInfo)
		{
			return Ok(await blocksRepository.CreateAsync(blockInfo));
		}

		[HttpGet]
		public async Task<ActionResult<Block[]>> ReadEntitiesAsync()
		{
			return await blocksRepository.Db.Blocks
				.ToArrayAsync();
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<BlockInfo>> ReadEntityAsync(
			[FromRoute] int id)
		{
			return await blocksRepository.GetAsync(id);
		}

		[HttpGet("tree")]
		public async Task<ActionResult<Block[]>> ReadEntitiesTreeAsync()
		{
			return await blocksRepository.GetAsTree();
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult<Block>> UpdateEntityAsync(
			[FromRoute] int id,
			[FromBody] BlockInfo block)
		{
			return Ok(await blocksRepository.UpdateAsync(id, block));
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult> DeleteEntityAsync(
			[FromRoute] int id)
		{
			await blocksRepository.DeleteAsync(id);

			return NoContent();
		}
	}
}
