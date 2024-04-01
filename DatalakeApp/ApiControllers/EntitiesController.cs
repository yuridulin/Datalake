namespace DatalakeApp.ApiControllers
{
	/*[Route("api/[controller]")]
	[ApiController]
	public class EntitiesController(DatalakeContext db) : ControllerBase
	{
		[HttpPost]
		public async Task<ActionResult<Block>> CreateEntityAsync(
			[FromBody] Block entity)
		{
			if (await db.Entities.AnyAsync(x => x.Name == entity.Name))
				return BadRequest("Сущность с таким именем уже существует");

			var id = await db.Entities
				.Value(x => x.GlobalId, entity.GlobalId)
				.Value(x => x.ParentId, entity.ParentId)
				.Value(x => x.Name, entity.Name)
				.Value(x => x.Description, entity.Description)
				.InsertWithInt32IdentityAsync();

			if (!id.HasValue)
				return BadRequest("Не удалось добавить сущность");
			else
				entity.Id = id.Value;

			return Ok(entity);
		}

		[HttpGet]
		public async Task<ActionResult<Block[]>> ReadEntitiesAsync()
		{
			return await db.Entities
				.ToArrayAsync();
		}

		[HttpGet("{id:int}")]
		public async Task<ActionResult<Block>> ReadEntityAsync(
			[FromRoute] int id)
		{
			var entity = await db.Entities.SingleOrDefaultAsync(x => x.Id == id);

			if (entity == null)
				return BadRequest($"Сущность #{id} не найдена");

			entity.Children = await db.Entities
				.Where(x => x.ParentId == entity.Id)
				.ToListAsync();

			return entity;
		}

		[HttpGet("tree")]
		public async Task<ActionResult<Block[]>> ReadEntitiesTreeAsync()
		{
			var entities = await db.Entities
				.ToArrayAsync();

			var top = entities
				.Where(x => !entities.Select(e => e.Id).ToList().Contains(x.ParentId))
				.ToArray();

			foreach (var entity in top)
			{
				entity.Children = ReadChildren(entity.Id);
			}

			return entities;

			Block[] ReadChildren(int id)
			{
				var children = entities.Where(x => x.ParentId == id).ToArray();

				foreach (var child in children)
				{
					child.Children = ReadChildren(child.Id);
				}

				return children;
			}
		}

		[HttpPut("{id:int}")]
		public async Task<ActionResult<Block>> UpdateEntityAsync(
			[FromRoute] int id,
			[FromBody] Block entity)
		{
			if (!await db.Entities.AnyAsync(x => x.Id == id))
				return BadRequest($"Сущность #{id} не найдена");
			if (await db.Entities.AnyAsync(x => x.Name == entity.Name))
				return BadRequest("Сущность с таким именем уже существует");

			await db.BeginTransactionAsync();

			int count = 0;

			count += await db.Entities
				.Where(x => x.Id == id)
				.Set(x => x.Name, entity.Name)
				.Set(x => x.Description, entity.Description)
				.Set(x => x.ParentId, entity.ParentId)
				.UpdateAsync();

			await db.EntityFields
				.Where(x => x.EntityId == id)
				.DeleteAsync();

			await db.BulkCopyAsync(entity.Fields);

			await db.EntityTags
				.Where(x => x.EntityId == id)
				.DeleteAsync();

			await db.BulkCopyAsync(entity.Tags);

			await db.CommitTransactionAsync();

			if (count == 0)
				return BadRequest($"Не удалось обновить сущность #{id}");

			return entity;
		}

		[HttpDelete("{id:int}")]
		public async Task<ActionResult> DeleteEntityAsync(
			[FromRoute] int id)
		{
			var count = await db.Entities
				.Where(x => x.Id == id)
				.DeleteAsync();

			if (count == 0)
				return BadRequest($"Не удалось удалить сущность #{id}");

			return Ok();
		}
	}*/
}
