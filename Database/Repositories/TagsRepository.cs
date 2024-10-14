using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Tags;
using Datalake.ApiClasses.Models.Users;
using Datalake.Database.Extensions;
using Datalake.Database.Models;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

public partial class TagsRepository(DatalakeContext db)
{
	public static Dictionary<int, TagCacheInfo> CachedTags { get; set; } = [];

	#region Действия

	public async Task<TagInfo> CreateAsync(
		UserAuthInfo user,
		TagCreateRequest tagCreateRequest,
		Guid? energoId = null)
	{
		if (tagCreateRequest.SourceId.HasValue)
		{
			await db.AccessRepository.CheckAccessToSource(user, AccessType.Admin, tagCreateRequest.SourceId.Value, energoId);
		}
		else if (tagCreateRequest.BlockId.HasValue)
		{
			await db.AccessRepository.CheckAccessToBlockAsync(user, AccessType.Admin, tagCreateRequest.BlockId.Value, energoId);
		}
		else
		{
			await db.AccessRepository.CheckGlobalAccess(user, AccessType.Admin, energoId);
		}

		return await CreateAsync(tagCreateRequest);
	}

	public async Task UpdateAsync(
		UserAuthInfo user,
		Guid guid,
		TagUpdateRequest updateRequest,
		Guid? energoId = null)
	{
		await db.AccessRepository.CheckAccessToTagAsync(user, AccessType.Admin, guid, energoId);
		await UpdateAsync(guid, updateRequest);
	}

	public async Task DeleteAsync(
		UserAuthInfo user,
		Guid guid,
		Guid? energoId = null)
	{
		await db.AccessRepository.CheckAccessToTagAsync(user, AccessType.Admin, guid, energoId);
		await DeleteAsync(guid);
	}

	#endregion

	#region Реализация

	static object locker = new();

	internal async Task<TagInfo> CreateAsync(TagCreateRequest createRequest)
	{
		// TODO: проверка разрешения на создание тега

		if (!createRequest.SourceId.HasValue && !createRequest.BlockId.HasValue)
			throw new InvalidValueException(message: "тег не может быть создан без привязок, нужно указать или источник, или блок");

		bool needToAddIdInName = string.IsNullOrEmpty(createRequest.Name);
		if (!string.IsNullOrEmpty(createRequest.Name))
		{
			createRequest.Name = createRequest.Name.RemoveWhitespaces("_");

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
				createRequest.SourceItem = createRequest.SourceItem.RemoveWhitespaces();
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
				needToAddIdInName = false;
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
				?? throw new NotFoundException(message: $"блок #{createRequest.BlockId}");

			// TODO: проверка разрешения на изменение блока

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

		if (createRequest.BlockId.HasValue)
		{
			await db.BlockTags
				.Value(x => x.TagId, tag.Id)
				.Value(x => x.BlockId, createRequest.BlockId)
				.Value(x => x.Name, createRequest.Name)
				.Value(x => x.Relation, BlockTagRelation.Static)
				.InsertAsync();
		}

		await db.LogsRepository.LogAsync(new Log
		{
			Category = LogCategory.Tag,
			RefId = tag.Id.ToString(),
			Text = $"Создан тег \"{createRequest.Name}\"",
			Type = LogType.Success,
		});

		await transaction.CommitAsync();

		await UpdateTagCache(tag.Id);
		var info = await GetInfoWithSources().FirstOrDefaultAsync(x => x.Id == tag.Id)
			?? throw new NotFoundException(message: "тег после создания");

		return info;
	}

	internal async Task UpdateAsync(Guid guid, TagUpdateRequest updateRequest)
	{
		var transaction = await db.BeginTransactionAsync();

		updateRequest.Name = ValueChecker.RemoveWhitespaces(updateRequest.Name, "_");

		var tag = await db.Tags.Where(x => x.GlobalGuid == guid).FirstOrDefaultAsync()
			?? throw new NotFoundException($"тег {guid}");

		if (await db.Tags.AnyAsync(x => x.GlobalGuid != guid && x.Name == updateRequest.Name))
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

		int count = await db.Tags
			.Where(x => x.GlobalGuid == guid)
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

		if (count != 1)
			throw new DatabaseException($"Не удалось сохранить тег {guid}", DatabaseStandartError.UpdatedZero);

		await db.TagInputs
			.Where(x => x.TagId == tag.Id)
			.DeleteAsync();

		await db.TagInputs
			.BulkCopyAsync(updateRequest.FormulaInputs.Select(x => new TagInput
			{
				TagId = tag.Id,
				InputTagId = x.Id,
				VariableName = x.VariableName,
			}));

		var updatedTag = await db.Tags.Where(x => x.GlobalGuid == guid).FirstOrDefaultAsync()
			?? throw new NotFoundException($"тег {guid}");

		await db.LogsRepository.LogAsync(new Log
		{
			Category = LogCategory.Tag,
			RefId = tag.Id.ToString(),
			Text = $"Изменен тег \"{tag.Name}\". " + ObjectExtension<Tag>.Difference(tag, updatedTag),
			Type = LogType.Success,
		});

		await transaction.CommitAsync();

		await UpdateTagCache(tag.Id);
	}

	internal async Task DeleteAsync(Guid guid)
	{
		var transaction = await db.BeginTransactionAsync();

		var tag = await db.Tags.Where(x => x.GlobalGuid == guid).FirstOrDefaultAsync()
			?? throw new NotFoundException($"тег {guid}");

		var cached = CachedTags.Values.FirstOrDefault(x => x.Guid == guid)
			?? throw new NotFoundException(message: $"тег {guid}");

		var count = await db.Tags
			.Where(x => x.GlobalGuid == guid)
			.DeleteAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось удалить тег {guid}", DatabaseStandartError.DeletedZero);

		// TODO: удаление истории тега. Так как доступ идёт по id, получить её после пересоздания не получится
		// Либо нужно сделать отслеживание соответствий локальный и глобальных id, и при получении истории обогащать выборку предыдущей историей

		await db.LogsRepository.LogAsync(new Log
		{
			Category = LogCategory.Tag,
			RefId = tag.Id.ToString(),
			Text = $"Удален тег \"{tag.Name}\".",
			Type = LogType.Success,
		});

		await transaction.CommitAsync();

		await UpdateTagCache(cached.Id);
	}

	internal async Task UpdateTagCache(int id)
	{
		var cache = await GetTagsForCache().FirstOrDefaultAsync(x => x.Id == id);
		lock (locker)
		{
			if (cache == null)
			{
				CachedTags.Remove(id);
			}
			else
			{
				CachedTags[id] = cache;
			}
		}

		SystemRepository.Update();
	}

	#endregion
}
