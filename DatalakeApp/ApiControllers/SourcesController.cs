using DatalakeApp.Services;
using DatalakeDatabase.ApiModels.Sources;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Repositories;
using LinqToDB;
using Microsoft.AspNetCore.Mvc;

namespace DatalakeApp.ApiControllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SourcesController(SourcesRepository sourcesRepository, ReceiverService receiverService) : ControllerBase
	{
		[HttpPost]
		public async Task<ActionResult> Create(
			[FromBody] SourceInfo source)
		{
			await sourcesRepository.CreateAsync(source);

			return NoContent();
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<SourceInfo>> Read(
			[FromRoute] int id)
		{
			var source = await sourcesRepository.Db.Sources
				.Where(x => x.Id == id)
				.Select(x => new SourceInfo
				{
					Id = x.Id,
					Name = x.Name,
					Address = x.Address,
					Description = x.Description
				})
				.FirstOrDefaultAsync()
				?? throw new NotFoundException($"Источник #{id} не найден");

			return NoContent();
		}

		[HttpGet]
		public async Task<ActionResult<SourceInfo[]>> ReadAll()
		{
			var sources = await sourcesRepository.Db.Sources
				.Select(x => new SourceInfo
				{
					Id = x.Id,
					Name = x.Name,
				})
				.ToArrayAsync();

			return Ok(sources);
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult> Update(
			[FromRoute] int id,
			[FromBody] SourceInfo source)
		{
			await sourcesRepository.UpdateAsync(id, source);

			return NoContent();
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult> Delete(
			[FromRoute] int id)
		{
			await sourcesRepository.DeleteAsync(id);

			return NoContent();
		}

		[HttpGet("{id:int}/tags")]
		public async Task<ActionResult<SourceEntryInfo[]>> GetSourceTags(
			[FromRoute] int id)
		{
			var source = await sourcesRepository.Db.Sources
				.Where(x => x.Id == id)
				.Select(x => new { x.Type, x.Address })
				.FirstOrDefaultAsync()
				?? throw new NotFoundException($"Источник #{id} не найден");

			var items = receiverService.GetItemsFromSourceAsync(source.Type, source.Address);

			return Ok(await sourcesRepository.GetExistTagsAsync(id));
		}
	}
}
