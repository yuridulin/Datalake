using DatalakeApp.Services.Receiver;
using DatalakeDatabase.ApiModels.Sources;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace DatalakeApp.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class SourcesController(
	SourcesRepository sourcesRepository,
	ReceiverService receiverService) : ControllerBase
{
	[HttpPost("empty")]
	public async Task<ActionResult<int>> CreateAsync()
	{
		var id = await sourcesRepository.CreateAsync();

		return id;
	}

	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[BindRequired, FromBody] SourceInfo source)
	{
		var id = await sourcesRepository.CreateAsync(source);

		return id;
	}

	[HttpGet("{id:int}")]
	public async Task<ActionResult<SourceInfo>> ReadAsync(
		[BindRequired, FromRoute] int id)
	{
		return await sourcesRepository.GetInfo()
			.FirstOrDefaultAsync(x => x.Id == id)
			?? throw new NotFoundException($"Источник #{id}");
	}

	[HttpGet]
	public async Task<ActionResult<SourceInfo[]>> ReadAsync(bool withCustom = false)
	{
		return await sourcesRepository.GetInfo(withCustom)
			.ToArrayAsync();
	}

	[HttpPut("{id:int}")]
	public async Task<ActionResult> UpdateAsync(
		[BindRequired, FromRoute] int id,
		[BindRequired, FromBody] SourceInfo source)
	{
		await sourcesRepository.UpdateAsync(id, source);

		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		await sourcesRepository.DeleteAsync(id);

		return NoContent();
	}

	[HttpGet("{id:int}/tags")]
	public async Task<ActionResult<SourceEntryInfo[]>> GetItemsWithTagsAsync(
		[BindRequired, FromRoute] int id)
	{
		var source = await sourcesRepository.GetInfo()
			.Where(x => x.Id == id)
			.Select(x => new { x.Type, x.Address })
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Источник #{id}");

		var sourceItemsResponse = await receiverService.GetItemsFromSourceAsync(source.Type, source.Address);
		var sourceItems = sourceItemsResponse.Tags
			.DistinctBy(x => x.Name)
			.ToDictionary(x => x.Name, x => new SourceItemInfo { Path = x.Name, Type = x.Type });

		var sourceTags = await sourcesRepository.GetExistTags(id)
			.ToListAsync();

		var all = sourceTags.Select(tag => new SourceEntryInfo
			{
				TagInfo = tag,
				ItemInfo = sourceItems.TryGetValue(tag.Item, out var itemInfo) ? itemInfo : null,
			})
			.Union(sourceItems
				.Where(itemKeyValue => !sourceTags.Select(tag => tag.Item).Contains(itemKeyValue.Key))
				.Select(itemKeyValue => new SourceEntryInfo
				{
					TagInfo = null,
					ItemInfo = itemKeyValue.Value,
				}))
			.ToArray();

		return all;
	}
}
