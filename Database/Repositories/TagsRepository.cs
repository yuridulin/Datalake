using Datalake.Database.Constants;
using Datalake.Database.Enums;
using Datalake.Database.Exceptions;
using Datalake.Database.Extensions;
using Datalake.Database.Models.Auth;
using Datalake.Database.Models.Tags;
using Datalake.Database.Tables;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с тегами
/// </summary>
public class TagsRepository(DatalakeContext db)
{
	#region Действия

	/// <summary>
	/// Создание нового тега
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="tagCreateRequest">Параметры нового тега</param>
	/// <returns>Информация о созданном теге</returns>
	public async Task<TagInfo> CreateAsync(
		UserAuthInfo user,
		TagCreateRequest tagCreateRequest)
	{
		if (tagCreateRequest.SourceId.HasValue && tagCreateRequest.SourceId.Value > 0)
			AccessRepository.ThrowIfNoAccessToSource(user, AccessType.Admin, tagCreateRequest.SourceId.Value);

		if (tagCreateRequest.BlockId.HasValue)
			AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Admin, tagCreateRequest.BlockId.Value);

		if ((!tagCreateRequest.SourceId.HasValue || tagCreateRequest.SourceId.Value <= 0) && !tagCreateRequest.BlockId.HasValue)
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		User = user.Guid;

		return await CreateAsync(tagCreateRequest);
	}

	/// <summary>
	/// Получение информации о теге
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="guid"></param>
	/// <returns></returns>
	public async Task<TagInfo> ReadAsync(UserAuthInfo user, Guid guid)
	{
		var rule = AccessRepository.GetAccessToTag(user, guid);
		if (!rule.AccessType.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var tag = await db.TagsRepository.GetInfoWithSources()
			.Where(x => x.Guid == guid)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Тег {guid}");

		tag.AccessRule = rule;

		return tag;
	}

	/// <summary>
	/// Получение информации о тегах с поддержкой фильтров
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="id">Идентификаторы тегов</param>
	/// <param name="names">Имена тегов</param>
	/// <param name="guids">Глобальные идентификаторы тегов</param>
	/// <returns>Список информации о тегах</returns>
	public async Task<TagInfo[]> ReadAllAsync(UserAuthInfo user, int? sourceId, int[]? id, string[]? names, Guid[]? guids)
	{
		var query = db.TagsRepository.GetInfoWithSources();

		if (sourceId.HasValue)
		{
			query = query.Where(x => sourceId.Value == x.SourceId);
		}
		if (id?.Length > 0)
		{
			query = query.Where(x => id.Contains(x.Id));
		}
		if (names?.Length > 0)
		{
			query = query.Where(x => names.Contains(x.Name));
		}
		if (guids?.Length > 0)
		{
			query = query.Where(x => guids.Contains(x.Guid));
		}

		var tags = await query.ToArrayAsync();

		foreach (var tag in tags)
		{
			tag.AccessRule = AccessRepository.GetAccessToTag(user, tag.Guid);
		}

		return tags.Where(x => x.AccessRule.AccessType.HasAccess(AccessType.Viewer)).ToArray();
	}

	/// <summary>
	/// Получение тегов, которые можно использовать как входные параметры для расчета значения
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="guid">Идентификатор тега</param>
	/// <returns>Список информации о тегах</returns>
	public async Task<TagAsInputInfo[]> ReadPossibleInputsAsync(UserAuthInfo user, Guid guid)
	{
		var tags = await GetPossibleInputs()
			.ToArrayAsync();

		// TODO: рекурсивный обход. Нужно исключить циклические зависимости
		if (guid.Equals(Guid.Empty))
		{ }

		foreach (var tag in tags)
		{
			tag.AccessRule = AccessRepository.GetAccessToTag(user, tag.Guid);
		}

		return tags.Where(x => x.AccessRule.AccessType.HasAccess(AccessType.Viewer)).ToArray();
	}

	/// <summary>
	/// Изменение параметров тега
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="guid">Идентификатор тега</param>
	/// <param name="updateRequest">Новые параметры тега</param>
	public async Task UpdateAsync(
		UserAuthInfo user,
		Guid guid,
		TagUpdateRequest updateRequest)
	{
		AccessRepository.ThrowIfNoAccessToTag(user, AccessType.Admin, guid);
		User = user.Guid;

		await UpdateAsync(guid, updateRequest);
	}

	/// <summary>
	/// Удаление тега
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="guid">Идентификатор тега</param>
	public async Task DeleteAsync(
		UserAuthInfo user,
		Guid guid)
	{
		AccessRepository.ThrowIfNoAccessToTag(user, AccessType.Admin, guid);
		User = user.Guid;

		await DeleteAsync(guid);
	}

	#endregion

	#region Кэш

	/// <summary>
	/// Кэшированный список тегов
	/// </summary>
	public static Dictionary<int, TagCacheInfo> CachedTags { get; set; } = [];

	static object locker = new();

	#endregion

	#region Реализация

	Guid? User { get; set; } = null;

	internal async Task<TagInfo> CreateAsync(TagCreateRequest createRequest)
	{
		if (!createRequest.SourceId.HasValue && !createRequest.BlockId.HasValue)
			throw new InvalidValueException(message: "тег не может быть создан без привязок, нужно указать или источник, или блок");

		bool needToAddIdInName = string.IsNullOrEmpty(createRequest.Name);
		if (!string.IsNullOrEmpty(createRequest.Name))
		{
			createRequest.Name = createRequest.Name.RemoveWhitespaces("_");

			if (await db.Tags.AnyAsync(x => x.Name.ToLower() == createRequest.Name.ToLower()))
				throw new ForbiddenException(message: "уже существует тег с таким именем");
		}

		if (createRequest.SourceId.HasValue)
		{
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
				createRequest.Name = (source.Id <= 0 ? ((SourceType)source.Id).ToString() : source.Name)
					+ "." + (createRequest.SourceItem ?? "Tag");

				if (createRequest.SourceId.Value > 0)
					needToAddIdInName = false;
			}
		}
		else
			throw new InvalidValueException(message: "необходимо выбрать источник");

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

			if (string.IsNullOrEmpty(createRequest.Name))
			{
				createRequest.Name = block.Name + ".Tag";
			}
		}

		using var transaction = await db.BeginTransactionAsync();

		var tag = new Tag
		{
			Created = DateFormats.GetCurrentDateTime(),
			GlobalGuid = Guid.NewGuid(),
			Frequency = createRequest.Frequency,
			IsScaling = false,
			Name = createRequest.Name!,
			SourceId = createRequest.SourceId ?? (int)SourceType.Manual,
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

		Guid? guid = await db.Tags.Where(x => x.Id == tag.Id).Select(x => x.GlobalGuid).FirstOrDefaultAsync();

		await LogAsync(guid, $"Создан тег \"{createRequest.Name}\"");

		await transaction.CommitAsync();

		await UpdateTagCache(tag.Id);
		var info = await GetInfoWithSources().FirstOrDefaultAsync(x => x.Id == tag.Id)
			?? throw new NotFoundException(message: "тег после создания");

		AccessRepository.Update();

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
			.Set(x => x.Frequency, updateRequest.Frequency)
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
				InputTagId = x.TagId,
				VariableName = x.VariableName,
			}));

		var updatedTag = await db.Tags.Where(x => x.GlobalGuid == guid).FirstOrDefaultAsync()
			?? throw new NotFoundException($"тег {guid}");

		await LogAsync(guid, $"Изменен тег \"{tag.Name}\"", ObjectExtension.Difference(tag, updatedTag));

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

		await LogAsync(guid, $"Удален тег \"{tag.Name}\"");

		await transaction.CommitAsync();

		await UpdateTagCache(cached.Id);

		AccessRepository.Update();
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

	internal async Task LogAsync(Guid? guid, string message, string? details = null)
	{
		await db.InsertAsync(new Log
		{
			Category = LogCategory.Tag,
			RefId = guid?.ToString() ?? null,
			Text = message,
			Type = LogType.Success,
			UserGuid = User,
			Details = details,
		});
	}

	#endregion

	#region Запросы

	internal IQueryable<TagInfo> GetInfoWithSources()
	{
		var query =
			from tag in db.Tags
			from source in db.Sources.LeftJoin(x => x.Id == tag.SourceId)
			select new TagInfo
			{
				Id = tag.Id,
				Guid = tag.GlobalGuid,
				Name = tag.Name,
				Description = tag.Description,
				Frequency = tag.Frequency,
				Type = tag.Type,
				Formula = tag.Formula ?? string.Empty,
				FormulaInputs = (
					from input_rel in db.TagInputs.LeftJoin(x => x.TagId == tag.Id)
					from input in db.Tags.InnerJoin(x => x.Id == input_rel.InputTagId)
					from input_source in db.Sources.LeftJoin(x => x.Id == input.SourceId)
					select new TagInputInfo
					{
						Id = input.Id,
						Guid = input.GlobalGuid,
						Name = input.Name,
						VariableName = input_rel.VariableName,
						Type = input.Type,
						Frequency = input.Frequency,
						SourceType = input_source != null ? input_source.Type : SourceType.NotSet,
					}
				).ToArray(),
				IsScaling = tag.IsScaling,
				MaxEu = tag.MaxEu,
				MaxRaw = tag.MaxRaw,
				MinEu = tag.MinEu,
				MinRaw = tag.MinRaw,
				SourceId = tag.SourceId,
				SourceItem = tag.SourceItem,
				SourceType = source != null ? source.Type : SourceType.NotSet,
				SourceName = source != null ? source.Name : "Unknown",
			};

		return query;
	}

	internal IQueryable<TagAsInputInfo> GetPossibleInputs()
	{
		var query =
			from tag in db.Tags
			from source in db.Sources.LeftJoin(x => x.Id == tag.SourceId)
			orderby tag.Name
			select new TagAsInputInfo
			{
				Id = tag.Id,
				Guid = tag.GlobalGuid,
				Name = tag.Name,
				Type = tag.Type,
				Frequency = tag.Frequency,
				SourceType = source != null ? source.Type : SourceType.NotSet,
			};

		return query;
	}

	internal IQueryable<TagCacheInfo> GetTagsForCache()
	{
		var query =
			from t in db.Tags
			from s in db.Sources.LeftJoin(x => x.Id == t.SourceId)
			select new TagCacheInfo
			{
				Id = t.Id,
				Guid = t.GlobalGuid,
				Name = t.Name,
				Type = t.Type,
				SourceType = s.Type,
				Frequency = t.Frequency,
				ScalingCoefficient = t.IsScaling
					? ((t.MaxEu - t.MinEu) / (t.MaxRaw - t.MinRaw))
					: 1,
			};

		return query;
	}

	#endregion
}
