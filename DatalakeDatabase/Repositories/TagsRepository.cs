using DatalakeDatabase.Enums;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Models;
using LinqToDB;

namespace DatalakeDatabase.Repositories
{
	public class TagsRepository(DatalakeContext db)
	{
		public DatalakeContext Db => db;

		public async Task<Tag> CreateAsync(Tag tag)
		{
			if (string.IsNullOrEmpty(tag.Name))
			{
				if (tag.SourceId == (int)CustomSource.System)
					throw new ForbiddenException("Запрещено создавать системные теги");

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
						.FirstOrDefaultAsync()
						?? throw new NotFoundException($"Указанный источник #{tag.SourceId} не найден");

					if (string.IsNullOrEmpty(tag.SourceItem))
						throw new InvalidValueException("Для несистемного источника обязателен путь к значению");

					tag.Name = $"{source.Name}.{tag.SourceItem}";
				}
			}
			else
			{
				if (tag.Name.Contains(' '))
					throw new InvalidValueException("В имени тега не разрешены пробелы");
				if (tag.SourceItem?.Contains(' ') ?? false)
					throw new InvalidValueException("В адресе значения не разрешены пробелы");
				if (await db.Tags.AnyAsync(x => x.Name == tag.Name))
					throw new AlreadyExistException("Уже существует тег с таким именем");
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
				.Value(x => x.MaxEu, tag.MaxEu)
				.Value(x => x.MinEu, tag.MinEu)
				.Value(x => x.MaxRaw, tag.MaxRaw)
				.Value(x => x.MinRaw, tag.MinRaw)
				.InsertWithInt32IdentityAsync();

			if (!id.HasValue)
				throw new DatabaseException("Не удалось добавить тег");
			else
				tag.Id = id.Value;

			return tag;
		}

		public async Task<Tag> UpdateAsync(int id, Tag tag)
		{
			if (tag.Name.Contains(' '))
				throw new InvalidValueException("В имени тега не разрешены пробелы");
			if (tag.SourceItem?.Contains(' ') ?? false)
				throw new InvalidValueException("В адресе значения не разрешены пробелы");
			if (!await db.Tags.AnyAsync(x => x.Id == id))
				throw new NotFoundException($"Тег #{id} не найден");
			if (await db.Tags.AnyAsync(x => x.Name == tag.Name))
				throw new AlreadyExistException("Уже существует тег с таким именем");

			int count = await db.Tags
				.Where(x => x.Id == id)
				.Set(x => x.Name, tag.Name)
				.Set(x => x.Description, tag.Description)
				.Set(x => x.Type, tag.Type)
				.Set(x => x.Created, tag.Created)
				.Set(x => x.SourceId, tag.SourceId)
				.Set(x => x.SourceItem, tag.SourceItem)
				.Set(x => x.IsScaling, tag.IsScaling)
				.Set(x => x.MaxEu, tag.MaxEu)
				.Set(x => x.MinEu, tag.MinEu)
				.Set(x => x.MaxRaw, tag.MaxRaw)
				.Set(x => x.MinRaw, tag.MinRaw)
				.UpdateAsync();

			if (count == 0)
				throw new DatabaseException($"Не удалось сохранить тег #{id}");

			return tag;
		}

		public async Task DeleteAsync(int id)
		{
			var count = await db.Tags
				.Where(x => x.Id == id)
				.DeleteAsync();

			if (count == 0)
				throw new DatabaseException($"Не удалось удалить тег #{id}");
		}
	}
}
