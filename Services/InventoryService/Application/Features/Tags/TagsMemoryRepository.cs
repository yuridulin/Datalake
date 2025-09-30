using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Tags;

namespace Datalake.InventoryService.Application.Features.Tags;

/// <summary>
/// Репозиторий тегов
/// </summary>
public class TagsMemoryRepository(InventoryCacheStore dataStore)
{
	#region API

	/// <summary>
	/// Создание нового тега
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="tagCreateRequest">Параметры нового тега</param>
	/// <returns>Информация о созданном теге</returns>
	public async Task<TagInfo> CreateAsync(
		InventoryEfContext db,
		UserAuthInfo user,
		TagCreateRequest tagCreateRequest)
	{
		if (tagCreateRequest.SourceId.HasValue && tagCreateRequest.SourceId.Value > 0)
			user.ThrowIfNoAccessToSource(AccessType.Manager, tagCreateRequest.SourceId.Value);

		if (tagCreateRequest.BlockId.HasValue)
			user.ThrowIfNoAccessToBlock(AccessType.Manager, tagCreateRequest.BlockId.Value);

		if ((!tagCreateRequest.SourceId.HasValue || tagCreateRequest.SourceId.Value <= 0) && !tagCreateRequest.BlockId.HasValue)
			user.ThrowIfNoGlobalAccess(AccessType.Manager);

		return await ProtectedCreateAsync(db, user.Guid, tagCreateRequest);
	}

	/// <summary>
	/// Получение информации о теге
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор тега</param>
	/// <returns></returns>
	public TagFullInfo Get(UserAuthInfo user, int id)
	{
		var rule = user.GetAccessToTag(id);
		if (!rule.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var tag = dataStore.State.TagsInfoFull()
			.FirstOrDefault(x => x.Id == id)
			?? throw new NotFoundException($"Тег {id}");

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
	public TagInfo[] GetAll(UserAuthInfo user, int? sourceId, int[]? id, string[]? names, Guid[]? guids)
	{
		var tagsChain = dataStore.State.TagsInfoWithSources();

		if (sourceId.HasValue)
		{
			tagsChain = tagsChain.Where(x => sourceId.Value == x.SourceId);
		}
		if (id?.Length > 0)
		{
			tagsChain = tagsChain.Where(x => id.Contains(x.Id));
		}
		if (names?.Length > 0)
		{
			tagsChain = tagsChain.Where(x => names.Contains(x.Name));
		}
		if (guids?.Length > 0)
		{
			tagsChain = tagsChain.Where(x => guids.Contains(x.Guid));
		}

		var tags = tagsChain.ToArray();

		List<TagInfo> tagsWithAccess = [];
		foreach (var tag in tags)
		{
			tag.AccessRule = user.GetAccessToTag(tag.Id);
			if (tag.AccessRule.HasAccess(AccessType.Viewer))
				tagsWithAccess.Add(tag);
		}

		return tagsWithAccess.ToArray();
	}

	/// <summary>
	/// Изменение параметров тега
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор тега</param>
	/// <param name="updateRequest">Новые параметры тега</param>
	public async Task UpdateAsync(
		InventoryEfContext db,
		UserAuthInfo user,
		int id,
		TagUpdateRequest updateRequest)
	{
		user.ThrowIfNoAccessToTag(AccessType.Manager, id);

		await ProtectedUpdateAsync(db, user.Guid, id, updateRequest);
	}

	/// <summary>
	/// Удаление тега
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор тега</param>
	public async Task DeleteAsync(
		InventoryEfContext db,
		UserAuthInfo user,
		int id)
	{
		user.ThrowIfNoAccessToTag(AccessType.Manager, id);

		await ProtectedDeleteAsync(db, user.Guid, id);
	}

	#endregion API

	#region Действия

	internal async Task<TagInfo> ProtectedCreateAsync(
		InventoryEfContext db,
		Guid userGuid,
		TagCreateRequest createRequest)
	{
		// Проверки, не требующие стейта
		bool needToAddIdInName = string.IsNullOrEmpty(createRequest.Name);

		if (!createRequest.SourceId.HasValue)
			throw new InvalidValueException(message: "необходимо выбрать источник");

		SourceEntity? source = null;
		BlockEntity? block = null;
		BlockTagEntity? relationToBlock = null;
		TagEntity createdTag;

		// Блокируем стейт до завершения обновления
		InventoryState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!createRequest.SourceId.HasValue && !createRequest.BlockId.HasValue)
				throw new InvalidValueException(message: "тег не может быть создан без привязок, нужно указать или источник, или блок");

			if (!needToAddIdInName)
			{
				if (currentState.Tags.Any(x => !x.IsDeleted && x.Name.ToLower() == createRequest.Name?.ToLower()))
					throw new ForbiddenException(message: "уже существует тег с таким именем");
			}

			if (createRequest.SourceId.HasValue)
			{
				if (!currentState.SourcesById.TryGetValue(createRequest.SourceId.Value, out source))
					throw new NotFoundException(message: $"источник #{createRequest.SourceId}");

				if (string.IsNullOrEmpty(createRequest.Name))
				{
					createRequest.Name = (source.Id <= 0 ? ((SourceType)source.Id).ToString() : source.Name)
						+ "." + (createRequest.SourceItem ?? "Tag");

					if (createRequest.SourceId.Value > 0)
						needToAddIdInName = false;
				}
			}

			if (createRequest.BlockId.HasValue)
			{
				if (!currentState.BlocksById.TryGetValue(createRequest.BlockId.Value, out block))
					throw new NotFoundException(message: $"блок #{createRequest.BlockId}");

				if (string.IsNullOrEmpty(createRequest.Name))
				{
					createRequest.Name = block.Name + ".Tag";
				}
			}

			createdTag = new TagEntity
			{
				Created = DateFormats.GetCurrentDateTime(),
				GlobalGuid = Guid.NewGuid(),
				Resolution = createRequest.Resolution,
				IsScaling = false,
				Name = createRequest.Name!,
				SourceId = createRequest.SourceId ?? (int)SourceType.Manual,
				Type = createRequest.TagType,
				SourceItem = createRequest.SourceItem,
			};

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				var newId = await db.InsertWithInt32IdentityAsync(createdTag);
				createdTag = createdTag with { Id = newId };

				if (needToAddIdInName)
				{
					createdTag.Name += createdTag.Id.ToString();

					await db.Tags
						.Where(x => x.Id == createdTag.Id)
						.Set(x => x.Name, createdTag.Name)
						.UpdateAsync();
				}

				if (block != null)
				{
					relationToBlock = new BlockTagEntity
					{
						BlockId = block.Id,
						TagId = createdTag.Id,
						Relation = BlockTagRelation.Static,
						Name = createdTag.Name,
					};

					await db.BlockTags
						.Value(x => x.TagId, relationToBlock.TagId)
						.Value(x => x.BlockId, relationToBlock.BlockId)
						.Value(x => x.Name, relationToBlock.Name)
						.Value(x => x.Relation, relationToBlock.Relation)
						.InsertAsync();
				}

				await LogAsync(db, userGuid, createdTag.Id, $"Создан тег \"{createRequest.Name}\"");

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось создать тег в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Blocks = state.Blocks,
				Tags = state.Tags.Add(createdTag),
				BlockTags = relationToBlock != null
					? state.BlockTags.Add(relationToBlock)
					: state.BlockTags,
			});
		}

		// Возвращение ответа
		var createdTagInfo = new TagInfo
		{
			Id = createdTag.Id,
			Guid = createdTag.Guid,
			Name = createdTag.Name,
			Description = createdTag.Description,
			Resolution = createdTag.Resolution,
			Type = createdTag.Type,
			Calculation = createdTag.Calculation,
			Formula = createdTag.Formula,
			Thresholds = createdTag.Thresholds,
			FormulaInputs = [], // их не может быть при создании
			IsScaling = createdTag.IsScaling,
			MaxEu = createdTag.MaxEu,
			MaxRaw = createdTag.MaxRaw,
			MinEu = createdTag.MinEu,
			MinRaw = createdTag.MinRaw,
			SourceId = createdTag.SourceId,
			SourceItem = createdTag.SourceItem,
			SourceType = source != null ? source.Type : SourceType.NotSet,
			SourceName = source != null ? source.Name : "Unknown",
			SourceTag = null, // его не может быть при создании
			Aggregation = createdTag.Aggregation,
			AggregationPeriod = createdTag.AggregationPeriod,
		};

		return createdTagInfo;
	}

	internal async Task ProtectedUpdateAsync(
		InventoryEfContext db,
		Guid userGuid,
		int id,
		TagUpdateRequest updateRequest)
	{
		// Проверки, не требующие стейта
		if (updateRequest.SourceId > 0)
		{
			if (string.IsNullOrEmpty(updateRequest.SourceItem))
				throw new InvalidValueException("Для несистемного источника обязателен путь к значению");
		}
		else
		{
			updateRequest.SourceItem = null;
		}

		// Блокируем стейт до завершения обновления
		using (await dataStore.AcquireWriteLockAsync())
		{
			// Проверки на актуальном стейте
			var state = dataStore.State;

			if (!state.TagsById.TryGetValue(id, out var tag))
				throw new NotFoundException($"тег {id}");

			var updatedTag = tag with
			{
				Name = updateRequest.Name,
				Description = updateRequest.Description,
				Type = updateRequest.Type,
				Resolution = updateRequest.Resolution,
				SourceId = updateRequest.SourceId,
				SourceItem = updateRequest.SourceItem,
				IsScaling = updateRequest.IsScaling,
				MaxEu = updateRequest.MaxEu,
				MinEu = updateRequest.MinEu,
				MaxRaw = updateRequest.MaxRaw,
				MinRaw = updateRequest.MinRaw,
				Calculation = updateRequest.Calculation,
				Formula = updateRequest.Formula,
				Thresholds = updateRequest.Thresholds?.ToList(),
				ThresholdSourceTagId = updateRequest.ThresholdSourceTagId,
				ThresholdSourceTagBlockId = updateRequest.ThresholdSourceTagBlockId,
				SourceTagId = updateRequest.SourceTagId,
				SourceTagBlockId = updateRequest.SourceTagBlockId,
				Aggregation = updateRequest.Aggregation,
				AggregationPeriod = updateRequest.AggregationPeriod,
			};

			if (state.Tags.Any(x => x.Id != id && !x.IsDeleted && x.Name == updateRequest.Name))
				throw new AlreadyExistException($"тег с именем {updateRequest.Name}");

			if (updateRequest.SourceTagId == tag.Id)
				throw new InvalidValueException("Тег не может быть источником значений для самого себя");

			List<string> changes = new();
			if (tag.Name != updateRequest.Name)
				changes.Add($"название: [{tag.Name}] > [{updateRequest.Name}]");
			if (tag.Description != updateRequest.Description)
				changes.Add($"описание: [{tag.Description}] > [{updateRequest.Description}]");
			if (tag.Type != updateRequest.Type)
				changes.Add($"тип значения: [{tag.Type}] > [{updateRequest.Type}]");
			if (tag.Resolution != updateRequest.Resolution)
				changes.Add($"частота: [{tag.Resolution}] > [{updateRequest.Resolution}]");
			if (tag.SourceId != updateRequest.SourceId)
				changes.Add($"источник: [{tag.SourceId}] > [{updateRequest.SourceId}]");
			if (tag.SourceItem != updateRequest.SourceItem)
				changes.Add($"путь в источнике: [{tag.SourceItem}] > [{updateRequest.SourceItem}]");
			if (tag.IsScaling != updateRequest.IsScaling)
				changes.Add($"шкалирование: [{tag.IsScaling}] > [{updateRequest.IsScaling}]");
			if (tag.MaxEu != updateRequest.MaxEu)
				changes.Add($"макс. знач. шкалы: [{tag.MaxEu}] > [{updateRequest.MaxEu}]");
			if (tag.MinEu != updateRequest.MinEu)
				changes.Add($"мин. знач. шкалы: [{tag.MinEu}] > [{updateRequest.MinEu}]");
			if (tag.MaxRaw != updateRequest.MaxRaw)
				changes.Add($"макс. знач. диапазона: [{tag.MaxRaw}] > [{updateRequest.MaxRaw}]");
			if (tag.MinRaw != updateRequest.MinRaw)
				changes.Add($"миню знач. диапазона: [{tag.MinRaw}] > [{updateRequest.MinRaw}]");
			if (tag.Formula != updateRequest.Formula)
				changes.Add($"формула: [{tag.Formula}] > [{updateRequest.Formula}]");
			if (tag.SourceTagId != updateRequest.SourceTagId)
				changes.Add($"тег-источник: [{tag.SourceTagId}] > [{updateRequest.SourceTagId}]");
			if (tag.Aggregation != updateRequest.Aggregation)
				changes.Add($"тип агрегации: [{tag.Aggregation}] > [{updateRequest.Aggregation}]");
			if (tag.AggregationPeriod != updateRequest.AggregationPeriod)
				changes.Add($"период агрегации: [{tag.AggregationPeriod}] > [{updateRequest.AggregationPeriod}]");

			var inputs = updateRequest.FormulaInputs
				.Select(x => new TagInputEntity
				{
					TagId = tag.Id,
					VariableName = x.VariableName,
					InputTagId = x.TagId,
					InputBlockId = x.BlockId,
				})
				.ToArray();

			List<string> addedInputs = new();
			List<string> updatedInputs = new();
			List<string> deletedInputs = new();
			foreach (var input in inputs)
			{
				var updated = updateRequest.FormulaInputs.FirstOrDefault(x => x.VariableName == input.VariableName);
				if (updated == null)
				{
					deletedInputs.Add(input.VariableName);
				}
				else if (input.InputTagId != updated.TagId)
				{
					updatedInputs.Add($"{input.VariableName}: [{input.InputTagId}] > [{updated.TagId}]");
				}
			}
			foreach (var updated in updateRequest.FormulaInputs)
			{
				if (inputs.Any(x => x.VariableName == updated.VariableName))
					continue;

				addedInputs.Add($"{updated.VariableName}: [{updated.TagId}]");
			}
			if (addedInputs.Count > 0 || updatedInputs.Count > 0 || deletedInputs.Count > 0)
			{
				string inputString = "входные параметры формулы: "
					+ (addedInputs.Count > 0 ? "\tдобавлены: " + string.Join(", ", addedInputs) : string.Empty)
					+ (updatedInputs.Count > 0 ? "\tизменены: " + string.Join(", ", updatedInputs) : string.Empty)
					+ (deletedInputs.Count > 0 ? "\tудалены: " + string.Join(", ", deletedInputs) : string.Empty);

				changes.Add(inputString);
			}

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				int records = await db.Tags
					.Where(x => x.Id == id)
					.Set(x => x.Name, updateRequest.Name)
					.Set(x => x.Description, updateRequest.Description)
					.Set(x => x.Type, updateRequest.Type)
					.Set(x => x.Resolution, updateRequest.Resolution)
					.Set(x => x.SourceId, updateRequest.SourceId)
					.Set(x => x.SourceItem, updateRequest.SourceItem)
					.Set(x => x.IsScaling, updateRequest.IsScaling)
					.Set(x => x.MaxEu, updateRequest.MaxEu)
					.Set(x => x.MinEu, updateRequest.MinEu)
					.Set(x => x.MaxRaw, updateRequest.MaxRaw)
					.Set(x => x.MinRaw, updateRequest.MinRaw)
					.Set(x => x.Calculation, updateRequest.Calculation)
					.Set(x => x.Formula, updateRequest.Formula)
					.Set(x => x.Thresholds, updateRequest.Thresholds)
					.Set(x => x.ThresholdSourceTagId, updateRequest.ThresholdSourceTagId)
					.Set(x => x.ThresholdSourceTagBlockId, updateRequest.ThresholdSourceTagBlockId)
					.Set(x => x.SourceTagId, updateRequest.SourceTagId)
					.Set(x => x.SourceTagBlockId, updateRequest.SourceTagBlockId)
					.Set(x => x.Aggregation, updateRequest.Aggregation)
					.Set(x => x.AggregationPeriod, updateRequest.AggregationPeriod)
					.UpdateAsync();

				if (records != 1)
					throw new DatabaseException($"Не удалось сохранить тег {id}", DatabaseStandartError.UpdatedZero);

				await db.TagInputs
					.Where(x => x.TagId == tag.Id)
					.DeleteAsync();

				if (inputs.Length > 0)
					await BulkCopyWithOutputAsync(db, inputs);

				await LogAsync(db, userGuid, tag.Id, $"Изменен тег \"{tag.Name}\"", string.Join(",\n", changes));

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось обновить тег в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Tags = state.Tags.Replace(tag, updatedTag),
				TagInputs = state.TagInputs.RemoveAll(x => x.TagId == tag.Id).AddRange(inputs),
			});
		}

		// Возвращение ответа
	}

	internal async Task ProtectedDeleteAsync(
		InventoryEfContext db,
		Guid userGuid,
		int id)
	{
		// Проверки, не требующие стейта

		// Блокируем стейт до завершения обновления
		using (await dataStore.AcquireWriteLockAsync())
		{
			// Проверки на актуальном стейте
			var state = dataStore.State;

			if (!state.TagsById.TryGetValue(id, out var tag))
				throw new NotFoundException($"тег {id}");

			var updatedTag = tag with { IsDeleted = true };

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				var count = await db.Tags
					.Where(x => x.Id == id)
					.Set(x => x.IsDeleted, true)
					.UpdateAsync();

				if (count == 0)
					throw new DatabaseException($"Не удалось удалить тег {tag.Name}", DatabaseStandartError.DeletedZero);

				// TODO: удаление истории тега. Так как доступ идёт по id, получить её после пересоздания не получится
				// Либо нужно сделать отслеживание соответствий локальный и глобальных id, и при получении истории обогащать выборку предыдущей историей

				await LogAsync(db, userGuid, tag.Id, $"Удален тег \"{tag.Name}\"");

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось удалить тег из БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Tags = state.Tags.Replace(tag, updatedTag),
			});
		}

		// Возвращение ответа
	}

	private static async Task LogAsync(InventoryEfContext db, Guid userGuid, int tagId, string message, string? details = null)
	{
		await db.InsertAsync(new Audit
		{
			Category = LogCategory.Tag,
			RefId = tagId.ToString(),
			AffectedTagId = tagId,
			Text = message,
			Type = LogType.Success,
			AuthorGuid = userGuid,
			Details = details,
		});
	}

	private static async Task BulkCopyWithOutputAsync(InventoryEfContext db, TagInputEntity[] newInputs)
	{
		// Формируем параметризованный SQL запрос
		var insertSql = $"""
			INSERT INTO "{TagInputEntity.TableName}"
			("{nameof(TagInputEntity.TagId)}", "{nameof(TagInputEntity.InputTagId)}", "{nameof(TagInputEntity.InputBlockId)}", "{nameof(TagInputEntity.VariableName)}")
			VALUES {string.Join(", ", newInputs.Select((_, i) =>
				$"(:t{i}, :it{i}, :ir{i}, :v{i})"))}
			RETURNING "{nameof(TagInputEntity.Id)}";
		""";

		// Создаем параметры
		var parameters = new List<DataParameter>();
		for (var i = 0; i < newInputs.Length; i++)
		{
			var item = newInputs[i];
			parameters.Add(new DataParameter($"t{i}", item.TagId));
			parameters.Add(new DataParameter($"it{i}", item.InputTagId));
			parameters.Add(new DataParameter($"ir{i}", item.InputBlockId));
			parameters.Add(new DataParameter($"v{i}", item.VariableName));
		}

		// Выполняем запрос и получаем Id
		var insertedIds = (await db.QueryToArrayAsync<int>(insertSql, parameters.ToArray()))
			.ToList();

		// Обновляем исходные объекты
		for (var i = 0; i < newInputs.Length; i++)
		{
			newInputs[i].Id = insertedIds[i];
		}
	}

	#endregion Действия
}