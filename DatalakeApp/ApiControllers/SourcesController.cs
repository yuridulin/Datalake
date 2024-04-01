namespace DatalakeApp.ApiControllers
{
	/*[Route("api/[controller]")]
	[ApiController]
	public class SourcesController(DatalakeContext db, ReceiverService receiverService) : ControllerBase
	{
		[HttpPost]
		public async Task<ActionResult<Source>> Create(
			[FromBody] Source source)
		{
			if (await db.Sources.AnyAsync(x => x.Name == source.Name))
				return BadRequest("Уже существует источник с таким именем");

			int? id = await db.Sources
				.Value(x => x.Name, source.Name)
				.Value(x => x.Address, source.Address)
				.Value(x => x.Type, source.Type)
				.InsertWithInt32IdentityAsync();

			if (!id.HasValue)
				return BadRequest("Не удалось добавить источник");

			return Ok(source);
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<Source>> Read(
			[FromRoute] int id)
		{
			var source = await db.Sources
				.FirstOrDefaultAsync(x => x.Id == id);

			if (source == null)
				return NotFound($"Источник #{id} не найден");

			return Ok(source);
		}

		[HttpGet]
		public async Task<ActionResult<Source[]>> ReadAll()
		{
			var sources = await db.Sources
				.ToArrayAsync();

			return Ok(sources);
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult<Source>> Update(
			[FromRoute] int id,
			[FromBody] Source source)
		{
			if (!await db.Sources.AnyAsync(x => x.Id == id))
				return NotFound($"Источник #{id} не найден");
			if (await db.Sources.AnyAsync(x => x.Name == source.Name))
				return BadRequest("Уже существует источник с таким именем");

			int count = await db.Sources
				.Where(x => x.Id == id)
				.Set(x => x.Name, source.Name)
				.Set(x => x.Address, source.Address)
				.Set(x => x.Type, source.Type)
				.UpdateAsync();

			if (count == 0)
				return BadRequest($"Не удалось обновить источник #{id}");

			return source;
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult> Delete(
			[FromRoute] int id)
		{
			var count = await db.Sources
				.Where(x => x.Id == id)
				.DeleteAsync();

			if (count == 0)
				return BadRequest($"Не удалось удалить источник #{id}");

			return Ok();
		}

		[HttpGet("{id:int}/tags")]
		public async Task<ActionResult<SourceRecord[]>> GetSourceTags(
			[FromRoute] int id)
		{
			var source = await db.Sources
				.Where(x => x.Id == id)
				.FirstOrDefaultAsync();

			if (source == null)
				return NotFound($"Источник #{id} не найден");

			var items = await receiverService.GetItemsFromSourceAsync(source);

			var tags = await db.Tags
					.Where(x => x.SourceId == id)
					.Where(x => items.Tags.Select(y => y.Name).Contains(x.SourceItem))
					.ToDictionaryAsync(x => x.SourceItem ?? "", x => x);

			var records = items.Tags
				.Select(x => new SourceRecord
				{
					Path = x.Name,
					Type = x.Type,
					RelatedTag = tags.TryGetValue(x.Name, out var t) ? t : null,
				})
				.ToArray();

			return Ok();
		}
	}*/
}
