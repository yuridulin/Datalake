using DatalakeDatabase.ApiModels.Tags;
using DatalakeDatabase.Enums;
using DatalakeDatabase.Exceptions;
using LinqToDB;

namespace DatalakeDatabase.Repositories
{
	public partial class TagsRepository(DatalakeContext db)
	{
		public async Task<int> CreateAsync(TagInfo tagInfo)
		{
			await CheckTagInfoAsync(tagInfo);

			int? id = await db.Tags
				.Value(x => x.GlobalId, Guid.NewGuid())
				.Value(x => x.Name, tagInfo.Name)
				.Value(x => x.Description, tagInfo.Description)
				.Value(x => x.Type, tagInfo.Type)
				.Value(x => x.Interval, tagInfo.Interval ?? 0)
				.Value(x => x.Created, DateTime.UtcNow)
				.Value(x => x.SourceId, tagInfo.SourceInfo.Id)
				.Value(x => x.SourceItem, tagInfo.SourceInfo.Item)
				.Value(x => x.IsScaling, tagInfo.MathInfo != null)
				.Value(x => x.MaxEu, tagInfo.MathInfo?.MaxEu)
				.Value(x => x.MinEu, tagInfo.MathInfo?.MinEu)
				.Value(x => x.MaxRaw, tagInfo.MathInfo?.MaxRaw)
				.Value(x => x.MinRaw, tagInfo.MathInfo?.MinRaw)
				.Value(x => x.IsCalculating, tagInfo.CalcInfo != null)
				.Value(x => x.Formula, tagInfo.CalcInfo?.Formula)
				.InsertWithInt32IdentityAsync();

			if (!id.HasValue)
				throw new DatabaseException("Не удалось добавить тег");
			
			if (string.IsNullOrEmpty(tagInfo.Name))
			{
				await CreateDefaultTagNameAsync(tagInfo);
				await db.Tags
					.Where(x => x.Id == id.Value)
					.Set(x => x.Name, tagInfo.Name)
					.UpdateAsync();
			}

			return id.Value;
		}

		public async Task UpdateAsync(int id, TagInfo tagInfo)
		{
			await CheckTagInfoAsync(tagInfo);

			int count = await db.Tags
				.Where(x => x.Id == id)
				.Set(x => x.Name, tagInfo.Name)
				.Set(x => x.Description, tagInfo.Description)
				.Set(x => x.Type, tagInfo.Type)
				.Set(x => x.Interval, tagInfo.Interval ?? 0)
				.Set(x => x.SourceId, tagInfo.SourceInfo?.Id)
				.Set(x => x.SourceItem, tagInfo.SourceInfo?.Item)
				.Set(x => x.IsScaling, tagInfo.MathInfo != null)
				.Set(x => x.MaxEu, tagInfo.MathInfo?.MaxEu)
				.Set(x => x.MinEu, tagInfo.MathInfo?.MinEu)
				.Set(x => x.MaxRaw, tagInfo.MathInfo?.MaxRaw)
				.Set(x => x.MinRaw, tagInfo.MathInfo?.MinRaw)
				.Set(x => x.IsCalculating, tagInfo.CalcInfo != null)
				.Set(x => x.Formula, tagInfo.CalcInfo?.Formula)
				.UpdateAsync();

			if (count == 0)
				throw new DatabaseException($"Не удалось сохранить тег #{id}");
		}

		public async Task DeleteAsync(int id)
		{
			var count = await db.Tags
				.Where(x => x.Id == id)
				.DeleteAsync();

			if (count == 0)
				throw new DatabaseException($"Не удалось удалить тег #{id}");
		}


		async Task CreateDefaultTagNameAsync(TagInfo tagInfo)
		{
			if (tagInfo.SourceInfo.Id == (int)CustomSource.Manual)
			{
				tagInfo.Name = $"ManualTag{tagInfo.Id}";
			}
			else if (tagInfo.SourceInfo.Id == (int)CustomSource.Calculated)
			{
				tagInfo.Name = $"CalcTag{tagInfo.Id}";
			}
			else if (tagInfo.SourceInfo.Id > 0)
			{
				var source = await db.Sources
					.Where(x => x.Id == tagInfo.SourceInfo.Id)
					.FirstOrDefaultAsync()
					?? throw new NotFoundException($"Указанный источник #{tagInfo.SourceInfo.Id} не найден");

				tagInfo.Name = $"{source.Name}.{tagInfo.SourceInfo.Item}";
			}
		}

		async Task CheckTagInfoAsync(TagInfo tagInfo)
		{
			if (tagInfo.Id.HasValue)
			{
				string exist = await db.Tags
					.Where(x => x.Id == tagInfo.Id)
					.Select(x => x.Name)
					.FirstOrDefaultAsync()
					?? throw new NotFoundException($"Тег #{tagInfo.Id} не найден");

				if (exist == tagInfo.Name)
					throw new AlreadyExistException($"Тег с именем {tagInfo.Name}");
			}

			if (tagInfo.Name?.Contains(' ') ?? false)
				throw new InvalidValueException("В имени тега не разрешены пробелы");

			if (tagInfo.SourceInfo.Id == (int)CustomSource.System)
				throw new ForbiddenException("Запрещено создавать системные теги");

			if (tagInfo.SourceInfo.Id > 0)
			{
				if (string.IsNullOrEmpty(tagInfo.SourceInfo.Item))
					throw new InvalidValueException("Для несистемного источника обязателен путь к значению");
				if (!tagInfo.Interval.HasValue || tagInfo.Interval.Value >= 0)
					throw new InvalidValueException("Интервал обновления должен быть неотрицательным целым числом");
			}

			if (tagInfo.SourceInfo.Item?.Contains(' ') ?? false)
				throw new InvalidValueException("В адресе значения не разрешены пробелы");
		}
	}
}
