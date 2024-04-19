using DatalakeDatabase.ApiModels.Tags;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Models;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeApp.ApiControllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class TagsController(TagsRepository tagsRepository) : ControllerBase
	{
		[HttpPost]
		public async Task<ActionResult<int>> CreateAsync(
			[FromBody] TagInfo tag)
		{
			return Ok(await tagsRepository.CreateAsync(tag));
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<TagInfo>> ReadAsync(int id)
		{
			var tag = await tagsRepository.GetTagsWithSources()
				.Where(x => x.Id == id)
				.FirstOrDefaultAsync()
				?? throw new NotFoundException($"Тег #{id}");

			return Ok(tag);
		}

		[HttpGet]
		public async Task<ActionResult<Tag[]>> ReadAsync()
		{
			var tags = await tagsRepository.GetTagsWithSources()
				.ToArrayAsync();

			return Ok(tags);
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult> UpdateAsync(
			[FromRoute] int id,
			[FromBody] TagInfo tag)
		{
			await tagsRepository.UpdateAsync(id, tag);

			return NoContent();
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult> DeleteAsync(
			[FromRoute] int id)
		{
			await tagsRepository.DeleteAsync(id);

			return NoContent();
		}
	}
}
