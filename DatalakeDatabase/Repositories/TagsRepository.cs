using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Tags;
using DatalakeApiClasses.Models.Users;
using DatalakeDatabase.Extensions;
using DatalakeDatabase.Models;
using DatalakeDatabase.Repositories.Base;
using LinqToDB;
using LinqToDB.Data;

namespace DatalakeDatabase.Repositories;

public partial class TagsRepository(DatalakeContext db) : RepositoryBase
{
	#region Действия

	public async Task<int> CreateAsync(UserAuthInfo user, TagCreateRequest tagCreateRequest)
	{
		if (tagCreateRequest.SourceId.HasValue)
		{
			CheckAccessToSource(user, AccessType.Admin, tagCreateRequest.SourceId.Value);
		}
		else if (tagCreateRequest.BlockId.HasValue)
		{
			await CheckAccessToBlockAsync(db, user, AccessType.Admin, tagCreateRequest.BlockId.Value);
		}
		else
		{
			CheckGlobalAccess(user, AccessType.Admin);
		}

		return await CreateAsync(tagCreateRequest);
	}

	public async Task UpdateAsync(UserAuthInfo user, int id, TagUpdateRequest updateRequest)
	{
		await CheckAccessToTagAsync(db, user, AccessType.Admin, id);
		await UpdateAsync(id, updateRequest);
	}

	public async Task DeleteAsync(UserAuthInfo user, int id)
	{
		await CheckAccessToTagAsync(db, user, AccessType.Admin, id);
		await DeleteAsync(id);
	}

	#endregion

	#region Реализация

	internal async Task<int> CreateAsync(TagCreateRequest createRequest)
	{
		// TODO: проверка разрешения на создание тега

		if (!createRequest.SourceId.HasValue && !createRequest.BlockId.HasValue)
			throw new InvalidValueException(message: "тег не может быть создан без привязок, нужно указать или источник, или родительскую сущность");

		bool needToAddIdInName = string.IsNullOrEmpty(createRequest.Name);
		if (!string.IsNullOrEmpty(createRequest.Name))
		{
			createRequest.Name = ValueChecker.RemoveWhitespaces(createRequest.Name, "_");

			#pragma warning disable CA1862
			if (await db.Tags.AnyAsync(x => x.Name.ToLower() == createRequest.Name.ToLower()))
				throw new ForbiddenException(message: "уже существует тег с таким именем");
			#pragma warning restore CA1862
		}

		if (createRequest.SourceId.HasValue)
		{
			if (createRequest.SourceId == (int)CustomSource.NotSet)
				throw new InvalidValueException(message: "необходимо выбрать источник");

			if (!string.IsNullOrEmpty(createRequest.SourceItem))
			{
				createRequest.SourceItem = ValueChecker.RemoveWhitespaces(createRequest.SourceItem);
			}

			var source = await db.Sources
				.Where(x => x.Id == createRequest.SourceId)
				.Select(x => new
				{
					x.Id,
					x.Name,
				})
				.FirstOrDefaultAsync()
				?? throw new NotFoundException(message: $"источник #{createRequest.SourceId}");

			if (string.IsNullOrEmpty(createRequest.Name))
			{
				createRequest.Name = (source.Id <= 0 ? ((CustomSource)source.Id).ToString() : source.Name) 
					+ "." + (createRequest.SourceItem ?? "Tag");
			}
		}
		
		if (createRequest.BlockId.HasValue)
		{
			var block = await db.Blocks
				.Where(x => x.Id == createRequest.BlockId)
				.Select(x => new
				{
					x.Id,
					x.Name,
				})
				.FirstOrDefaultAsync()
				?? throw new NotFoundException(message: $"сущность #{createRequest.BlockId}");

			// TODO: проверка разрешения на изменение сущности

			if (string.IsNullOrEmpty(createRequest.Name))
			{
				createRequest.Name = block.Name + ".Tag";
			}
		}

		using var transaction = await db.BeginTransactionAsync();

		var tag = new Tag
		{
			Created = DateTime.Now,
			GlobalGuid = Guid.NewGuid(),
			Interval = 60,
			IsScaling = false,
			Name = createRequest.Name!,
			SourceId = createRequest.SourceId ?? (int)CustomSource.Manual,
			Type = createRequest.TagType,
			SourceItem = createRequest.SourceItem,
		};
		tag.Id = await db.InsertWithInt32IdentityAsync(tag);

		if (needToAddIdInName)
		{
			createRequest.Name += tag.Id.ToString();

			await db.Tags
				.Where(x => x.Id == tag.Id)
				.Set(x => x.Name, createRequest.Name)
				.UpdateAsync();
		}

		await new ValuesRepository(db).InitializeValueAsync(tag.Id);

		if (createRequest.BlockId.HasValue)
		{
			await db.BlockTags
				.Value(x => x.TagId, tag.Id)
				.Value(x => x.BlockId, createRequest.BlockId)
				.Value(x => x.Name, createRequest.Name)
				.Value(x => x.Relation, BlockTagRelation.Static)
				.InsertAsync();
		}

		await db.LogAsync(new Log
		{
			Category = LogCategory.Tag,
			RefId = tag.Id.ToString(),
			Text = $"Создан тег \"{createRequest.Name}\"",
			Type = LogType.Success,
		});

		await db.SetLastUpdateToNowAsync();
		await transaction.CommitAsync();

		return tag.Id;
	}

	internal async Task UpdateAsync(int id, TagUpdateRequest updateRequest)
	{
		updateRequest.Name = ValueChecker.RemoveWhitespaces(updateRequest.Name, "_");

		if (!await db.Tags.AnyAsync(x => x.Id == id))
			throw new NotFoundException($"тег #{id}");

		if (await db.Tags.AnyAsync(x => x.Id != id && x.Name == updateRequest.Name))
			throw new AlreadyExistException($"тег с именем {updateRequest.Name}");

		if (updateRequest.SourceId > 0)
		{
			if (string.IsNullOrEmpty(updateRequest.SourceItem))
				throw new InvalidValueException("Для несистемного источника обязателен путь к значению");
			if (updateRequest.IntervalInSeconds < 0)
				throw new InvalidValueException("интервал обновления должен быть неотрицательным целым числом");

			updateRequest.SourceItem = ValueChecker.RemoveWhitespaces(updateRequest.SourceItem);
		}
		else
		{
			updateRequest.SourceItem = null;
		}

		var transaction = await db.BeginTransactionAsync();

		int count = await db.Tags
			.Where(x => x.Id == id)
			.Set(x => x.Name, updateRequest.Name)
			.Set(x => x.Description, updateRequest.Description)
			.Set(x => x.Type, updateRequest.Type)
			.Set(x => x.Interval, updateRequest.IntervalInSeconds)
			.Set(x => x.SourceId, updateRequest.SourceId)
			.Set(x => x.SourceItem, updateRequest.SourceItem)
			.Set(x => x.IsScaling, updateRequest.IsScaling)
			.Set(x => x.MaxEu, updateRequest.MaxEu)
			.Set(x => x.MinEu, updateRequest.MinEu)
			.Set(x => x.MaxRaw, updateRequest.MaxRaw)
			.Set(x => x.MinRaw, updateRequest.MinRaw)
			.Set(x => x.Formula, updateRequest.Formula)
			.UpdateAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось сохранить тег #{id}");

		await db.TagInputs
			.Where(x => x.TagId == id)
			.DeleteAsync();

		await db.TagInputs
			.BulkCopyAsync(updateRequest.FormulaInputs.Select(x => new TagInput
			{
				TagId = id,
				InputTagId = x.Id,
				VariableName = x.VariableName,
			}));

		await db.SetLastUpdateToNowAsync();
		await transaction.CommitAsync();
	}

	internal async Task DeleteAsync(int id)
	{
		var transaction = await db.BeginTransactionAsync();

		var count = await db.Tags
			.Where(x => x.Id == id)
			.DeleteAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось удалить тег #{id}");

		// TODO: удаление истории тега. Так как доступ идёт по id, получить её после пересоздания не получится
		// Либо нужно сделать отслеживание соответствий локальный и глобальных id, и при получении истории обогащать выборку предыдущей историей

		await db.SetLastUpdateToNowAsync();
		await transaction.CommitAsync();
	}

	#endregion
}
