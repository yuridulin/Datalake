namespace DatalakeApp.ApiControllers
{
	/*[ApiController]
	[Route("api/[controller]")]
	public class TagsController(DatalakeContext db) : ControllerBase
	{
		[HttpPost]
		public async Task<ActionResult<Tag>> Create(
			[FromBody] Tag tag)
		{
			if (string.IsNullOrEmpty(tag.Name))
			{
				if (tag.SourceId == (int)CustomSource.System)
					return BadRequest("Запрещено создавать системные теги");

				if (tag.SourceId == (int)CustomSource.Manual)
				{
					tag.Name = $"ManualTag{tag.Id}";
				}
				else if (tag.SourceId == (int)CustomSource.Calculated)
				{
					tag.Name = $"CalcTag{tag.Id}";
				}
				else if (tag.SourceId > 0)
				{
					var source = await db.Sources
						.Where(x => x.Id == tag.SourceId)
						.FirstOrDefaultAsync();

					if (source == null)
						return BadRequest($"Указанный источник #{tag.SourceId} не найден");

					if (string.IsNullOrEmpty(tag.SourceItem))
						return BadRequest("Для несистемного источника обязателен путь к значению");

					tag.Name = $"{source.Name}.{tag.SourceItem}";
				}
			}

			int? id = await db.Tags
				.Value(x => x.GlobalId, Guid.NewGuid())
				.Value(x => x.Name, tag.Name)
				.Value(x => x.Description, tag.Description)
				.Value(x => x.Type, tag.Type)
				.Value(x => x.Created, tag.Created)
				.Value(x => x.SourceId, tag.SourceId)
				.Value(x => x.SourceItem, tag.SourceItem)
				.Value(x => x.IsScaling, tag.IsScaling)
				.Value(x => x.MaxEU, tag.MaxEU)
				.Value(x => x.MinEU, tag.MinEU)
				.Value(x => x.MaxRaw, tag.MaxRaw)
				.Value(x => x.MinRaw, tag.MinRaw)
				.InsertWithInt32IdentityAsync();

			if (!id.HasValue)
				return BadRequest("Не удалось добавить тег");
			else
				tag.Id = id.Value;

			return tag;
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<Tag>> Read(int id)
		{
			var tag = await db.Tags
				.Where(x => x.Id == id)
				.FirstOrDefaultAsync();

			if (tag == null)
				return BadRequest($"Тег #{id} не найден");

			return Ok(tag);
		}

		[HttpGet]
		public async Task<ActionResult<Tag[]>> ReadAll()
		{
			var tags = await db.Tags
				.ToArrayAsync();

			return Ok(tags);
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult<Tag>> Update(
			[FromRoute] int id,
			[FromBody] Tag tag)
		{
			if (tag.Name.Contains(' '))
				return BadRequest("В имени тега не разрешены пробелы");
			if (tag.SourceItem?.Contains(' ') ?? false)
				return BadRequest("В адресе значения не разрешены пробелы");

			if (!await db.Tags.AnyAsync(x => x.Id == id))
				return BadRequest($"Тег #{id} не найден");

			int count = await db.Tags
				.Where(x => x.Id == id)
				.Set(x => x.Name, tag.Name)
				.Set(x => x.Description, tag.Description)
				.Set(x => x.Type, tag.Type)
				.Set(x => x.Created, tag.Created)
				.Set(x => x.SourceId, tag.SourceId)
				.Set(x => x.SourceItem, tag.SourceItem)
				.Set(x => x.IsScaling, tag.IsScaling)
				.Set(x => x.MaxEU, tag.MaxEU)
				.Set(x => x.MinEU, tag.MinEU)
				.Set(x => x.MaxRaw, tag.MaxRaw)
				.Set(x => x.MinRaw, tag.MinRaw)
				.UpdateAsync();

			if (count == 0)
				return BadRequest($"Не удалось сохранить тег #{id}");

			return Ok(tag);
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult> Delete(
			[FromRoute] int id)
		{
			var count = await db.Tags
				.Where(x => x.Id == id)
				.DeleteAsync();

			if (count == 0)
				return BadRequest($"Не удалось удалить тег #{id}");

			return Ok();
		}
	}*/
}
