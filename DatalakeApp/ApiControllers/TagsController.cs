using DatalakeDatabase.ApiModels.Tags;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatalakeApp.ApiControllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController(TagsRepository tagsRepository) : ControllerBase
{
	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[BindRequired, FromBody] TagInfo tag)
	{
		return await tagsRepository.CreateAsync(tag);
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<TagInfo>> ReadAsync(int id)
	{
		var tag = await tagsRepository.GetTagsWithSources()
			.Where(x => x.Id == id)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Тег #{id}");

		return tag;
	}

	[HttpGet]
	public async Task<ActionResult<TagInfo[]>> ReadAsync()
	{
		var tags = await tagsRepository.GetTagsWithSources()
			.ToArrayAsync();

		return tags;
	}

	[HttpPut("{id:int}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] TagInfo tag)
	{
		await tagsRepository.UpdateAsync(id, tag);

		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		await tagsRepository.DeleteAsync(id);

		return NoContent();
	}
}
