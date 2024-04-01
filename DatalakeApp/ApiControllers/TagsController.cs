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
		public async Task<ActionResult<Tag>> Create(
			[FromBody] Tag tag)
		{
			return Ok(await tagsRepository.CreateAsync(tag));
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<Tag>> Read(int id)
		{
			var tag = await tagsRepository.Db.Tags
				.Where(x => x.Id == id)
				.FirstOrDefaultAsync();

			if (tag == null)
				return BadRequest($"Тег #{id} не найден");

			return Ok(tag);
		}

		[HttpGet]
		public async Task<ActionResult<Tag[]>> ReadAll()
		{
			var tags = await tagsRepository.Db.Tags
				.ToArrayAsync();

			return Ok(tags);
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult<Tag>> Update(
			[FromRoute] int id,
			[FromBody] Tag tag)
		{
			return Ok(await tagsRepository.UpdateAsync(id, tag));
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult> Delete(
			[FromRoute] int id)
		{
			await tagsRepository.DeleteAsync(id);

			return NoContent();
		}
	}
}
