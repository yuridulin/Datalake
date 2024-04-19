using DatalakeApp.Services.Receiver;
using DatalakeDatabase.ApiModels.Sources;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeApp.ApiControllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SourcesController(
		SourcesRepository sourcesRepository,
		ReceiverService receiverService) : ControllerBase
	{
		[HttpPost]
		public async Task<ActionResult<int>> CreateAsync(
			[FromBody] SourceInfo source)
		{
			var id = await sourcesRepository.CreateAsync(source);

			return id;
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<SourceInfo>> ReadAsync(
			[FromRoute] int id)
		{
			return await sourcesRepository.GetSources()
				.FirstOrDefaultAsync(x => x.Id == id)
				?? throw new NotFoundException($"Источник #{id}");
		}

		[HttpGet]
		public async Task<ActionResult<SourceInfo[]>> ReadAsync()
		{
			return await sourcesRepository.GetSources()
				.ToArrayAsync();
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult> UpdateAsync(
			[FromRoute] int id,
			[FromBody] SourceInfo source)
		{
			await sourcesRepository.UpdateAsync(id, source);

			return NoContent();
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult> DeleteAsync(
			[FromRoute] int id)
		{
			await sourcesRepository.DeleteAsync(id);

			return NoContent();
		}

		[HttpGet("{id:int}/tags")]
		public async Task<ActionResult<SourceEntryInfo[]>> GetItemsWithTagsAsync(
			[FromRoute] int id)
		{
			var source = await sourcesRepository.GetSources()
				.Where(x => x.Id == id)
				.Select(x => new { x.Type, x.Address })
				.FirstOrDefaultAsync()
				?? throw new NotFoundException($"Источник #{id}");

			var items = await receiverService.GetItemsFromSourceAsync(source.Type, source.Address);
			var tags = await sourcesRepository.GetExistTags(id)
				.ToDictionaryAsync(x => x.Item, x => x);

			// склеить
			var all = items.Tags
				.Select(x => new SourceEntryInfo
				{
					ItemInfo = new SourceItemInfo
					{
						Path = x.Name,
						Type = x.Type,
					},
					TagInfo = tags.TryGetValue(x.Name, out SourceTagInfo? value) ? value : null,
				})
				.Union(tags
					.Where(x => !items.Tags.Select(x => x.Name).Contains(x.Key))
					.Select(x => new SourceEntryInfo
					{
						ItemInfo = null,
						TagInfo = x.Value,
					})
				)
				.ToArray();

			return all;
		}
	}
}
