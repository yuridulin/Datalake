using DatalakeDatabase.ApiModels.Tags;
using DatalakeDatabase.Enums;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Extensions;
using DatalakeDatabase.Helpers;
using DatalakeDatabase.Models;
using LinqToDB;

namespace DatalakeDatabase.Repositories;

public partial class TagsRepository(DatalakeContext db)
{
	public async Task<int> CreateAsync(TagCreateRequest tagCreateRequest)
	{
		// TODO: проверка разрешения на создание тега

		if (!tagCreateRequest.SourceId.HasValue && !tagCreateRequest.BlockId.HasValue)
			throw new InvalidValueException(message: "тег не может быть создан без привязок, нужно указать или источник, или родительскую сущность");

		bool needToAddIdInName = string.IsNullOrEmpty(tagCreateRequest.Name);
		if (!string.IsNullOrEmpty(tagCreateRequest.Name))
		{
			tagCreateRequest.Name = ValueChecker.RemoveWhitespaces(tagCreateRequest.Name, "_");

			#pragma warning disable CA1862
			if (await db.Tags.AnyAsync(x => x.Name.ToLower() == tagCreateRequest.Name.ToLower()))
				throw new ForbiddenException(message: "уже существует тег с таким именем");
			#pragma warning restore CA1862
		}

		if (tagCreateRequest.SourceId.HasValue)
		{
			if (!string.IsNullOrEmpty(tagCreateRequest.SourceItem))
			{
				tagCreateRequest.SourceItem = ValueChecker.RemoveWhitespaces(tagCreateRequest.SourceItem);
			}

			var source = await db.Sources
				.Where(x => x.Id == tagCreateRequest.SourceId)
				.Select(x => new
				{
					x.Id,
					x.Name,
				})
				.FirstOrDefaultAsync()
				?? throw new NotFoundException(message: $"источник #{tagCreateRequest.SourceId}");

			if (string.IsNullOrEmpty(tagCreateRequest.Name))
			{
				tagCreateRequest.Name = (source.Id <= 0 ? ((CustomSource)source.Id).ToString() : source.Name) 
					+ "." + (tagCreateRequest.SourceItem ?? "Tag");
			}
		}
		
		if (tagCreateRequest.BlockId.HasValue)
		{
			var block = await db.Blocks
				.Where(x => x.Id == tagCreateRequest.BlockId)
				.Select(x => new
				{
					x.Id,
					x.Name,
				})
				.FirstOrDefaultAsync()
				?? throw new NotFoundException(message: $"сущность #{tagCreateRequest.BlockId}");

			// TODO: проверка разрешения на изменение сущности

			if (string.IsNullOrEmpty(tagCreateRequest.Name))
			{
				tagCreateRequest.Name = block.Name + ".Tag";
			}
		}

		var transaction = await db.BeginTransactionAsync();

		int? id = await db.InsertWithInt32IdentityAsync(new Tag
		{
			Created = DateTime.Now,
			GlobalId = Guid.NewGuid(),
			Name = tagCreateRequest.Name!,
			Type = tagCreateRequest.TagType,
			Interval = 60,
			IsScaling = false,
			SourceId = tagCreateRequest.SourceId ?? (int)CustomSource.Manual,
			SourceItem = tagCreateRequest.SourceItem,
		});

		if (!id.HasValue)
			throw new DatabaseException(message: "не удалось добавить тег");

		if (needToAddIdInName)
		{
			tagCreateRequest.Name += id.Value.ToString();

			await db.Tags
				.Where(x => x.Id == id.Value)
				.Set(x => x.Name, tagCreateRequest.Name)
				.UpdateAsync();
		}

		await new ValuesRepository(db).InitializeValueAsync(id.Value);

		if (tagCreateRequest.BlockId.HasValue)
		{
			await db.BlockTags
				.Value(x => x.TagId, id.Value)
				.Value(x => x.BlockId, tagCreateRequest.BlockId)
				.Value(x => x.Name, tagCreateRequest.Name)
				.Value(x => x.Relation, BlockTagRelation.Static)
				.InsertAsync();
		}

		await db.LogAsync(new Log
		{
			Category = LogCategory.Tag,
			RefId = id.Value,
			Text = $"Создан тег \"{tagCreateRequest.Name}\"",
			Type = LogType.Success,
		});

		await db.UpdateAsync();

		await db.CommitTransactionAsync();

		return id.Value;
	}

	public async Task UpdateAsync(int id, TagInfo tagInfo)
	{
		tagInfo.Name = ValueChecker.RemoveWhitespaces(tagInfo.Name, "_");

		if (!await db.Tags.AnyAsync(x => x.Id == id))
			throw new NotFoundException($"тег #{tagInfo.Id}");

		if (await db.Tags.AnyAsync(x => x.Id != id && x.Name == tagInfo.Name))
			throw new AlreadyExistException($"тег с именем {tagInfo.Name}");

		if (tagInfo.SourceInfo.Id > 0)
		{
			if (string.IsNullOrEmpty(tagInfo.SourceInfo.Item))
				throw new InvalidValueException("Для несистемного источника обязателен путь к значению");
			if (tagInfo.IntervalInSeconds < 0)
				throw new InvalidValueException("интервал обновления должен быть неотрицательным целым числом");

			tagInfo.SourceInfo.Item = ValueChecker.RemoveWhitespaces(tagInfo.SourceInfo.Item);
		}
		else
		{
			tagInfo.SourceInfo.Item = null;
		}

		int count = await db.Tags
			.Where(x => x.Id == id)
			.Set(x => x.Name, tagInfo.Name)
			.Set(x => x.Description, tagInfo.Description)
			.Set(x => x.Type, tagInfo.Type)
			.Set(x => x.Interval, tagInfo.IntervalInSeconds)
			.Set(x => x.SourceId, tagInfo.SourceInfo?.Id)
			.Set(x => x.SourceItem, tagInfo.SourceInfo?.Item)
			.Set(x => x.IsScaling, tagInfo.MathInfo?.IsScaling ?? false)
			.Set(x => x.MaxEu, tagInfo.MathInfo?.MaxEu)
			.Set(x => x.MinEu, tagInfo.MathInfo?.MinEu)
			.Set(x => x.MaxRaw, tagInfo.MathInfo?.MaxRaw)
			.Set(x => x.MinRaw, tagInfo.MathInfo?.MinRaw)
			.Set(x => x.Formula, tagInfo.CalcInfo?.Formula)
			.UpdateAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось сохранить тег #{id}");

		await db.UpdateAsync();
	}

	public async Task DeleteAsync(int id)
	{
		var count = await db.Tags
			.Where(x => x.Id == id)
			.DeleteAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось удалить тег #{id}");

		// TODO: удаление истории тега. Так как доступ идёт по id, получить её после пересоздания не получится
		// Либо нужно сделать отслеживание соответствий локальный и глобальных id, и при получении истории обогащать выборку предыдущей историей

		await db.UpdateAsync();
	}
}
