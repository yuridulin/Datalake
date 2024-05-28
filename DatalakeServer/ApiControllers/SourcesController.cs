using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Sources;
using DatalakeServer.Services.Receiver;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using DatalakeServer.ApiControllers.Base;

namespace DatalakeServer.ApiControllers;

[Route("api/[controller]")]
[ApiController]
public class SourcesController(
	SourcesRepository sourcesRepository,
	ReceiverService receiverService) : ApiControllerBase
{
	[HttpPost("empty")]
	public async Task<ActionResult<int>> CreateAsync()
	{
		var user = Authenticate();

		var id = await sourcesRepository.CreateAsync(user);

		return id;
	}

	[HttpPost]
	public async Task<ActionResult<int>> CreateAsync(
		[BindRequired, FromBody] SourceInfo source)
	{
		var user = Authenticate();
		var id = await sourcesRepository.CreateAsync(user, source);

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
		var user = Authenticate();
		await sourcesRepository.UpdateAsync(user, id, source);

		return NoContent();
	}

	[HttpDelete("{id:int}")]
	public async Task<ActionResult> DeleteAsync(
		[BindRequired, FromRoute] int id)
	{
		var user = Authenticate();
		await sourcesRepository.DeleteAsync(user, id);

		return NoContent();
	}

	[HttpGet("{id:int}/items")]
	public async Task<ActionResult<SourceItemInfo[]>> GetItemsAsync(
		[BindRequired, FromRoute] int id)
	{
		var source = await sourcesRepository.GetInfo()
			.Where(x => x.Id == id)
			.Select(x => new { x.Type, x.Address })
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Источник #{id}");

		var sourceItemsResponse = await receiverService.GetItemsFromSourceAsync(source.Type, source.Address);

		var items = sourceItemsResponse.Tags
			.Select(x => new SourceItemInfo { Type = x.Type, Path = x.Name })
			.ToArray();

		return items;
	}

	[HttpGet("{id:int}/items-and-tags")]
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
