using Datalake.Database.Extensions;
using Datalake.Database.Tables;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Tags;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.InMemory.Repositories;

/// <summary>
/// Репозиторий тегов
/// </summary>
public class TagsMemoryRepository(DatalakeStateHolder stateHolder)
{
	internal async Task<TagInfo> CreateAsync(
		DatalakeContext db,
		Guid userGuid,
		TagCreateRequest createRequest)
	{
		// 1. Проверка версии данных, на основе которых сделан запрос

		var state = stateHolder.CurrentState;

		if (!createRequest.SourceId.HasValue && !createRequest.BlockId.HasValue)
			throw new InvalidValueException(message: "тег не может быть создан без привязок, нужно указать или источник, или блок");

		bool needToAddIdInName = string.IsNullOrEmpty(createRequest.Name);
		if (!string.IsNullOrEmpty(createRequest.Name))
		{
			createRequest.Name = createRequest.Name.RemoveWhitespaces("_");

			if (state.Tags.Any(x => !x.IsDeleted && x.Name.ToLower() == createRequest.Name.ToLower()))
				throw new ForbiddenException(message: "уже существует тег с таким именем");
		}

		Source? source = null;
		Block? block = null;
		BlockTag? relationToBlock = null;
		if (createRequest.SourceId.HasValue)
		{
			if (!string.IsNullOrEmpty(createRequest.SourceItem))
			{
				createRequest.SourceItem = createRequest.SourceItem.RemoveWhitespaces();
			}

			source = state.Sources.FirstOrDefault(x => x.Id == createRequest.SourceId.Value && !x.IsDeleted)
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
			block = state.Blocks.FirstOrDefault(x => x.Id == createRequest.BlockId.Value && !x.IsDeleted)
				?? throw new NotFoundException(message: $"блок #{createRequest.BlockId}");

			if (string.IsNullOrEmpty(createRequest.Name))
			{
				createRequest.Name = block.Name + ".Tag";
			}
		}

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

		using var transaction = await db.BeginTransactionAsync();

		try
		{
			// 2. Обновление в БД
			var newId = await db.InsertWithInt32IdentityAsync(tag);
			tag = tag with { Id = newId };

			if (needToAddIdInName)
			{
				createRequest.Name += tag.Id.ToString();

				await db.Tags
					.Where(x => x.Id == tag.Id)
					.Set(x => x.Name, createRequest.Name)
					.UpdateAsync();
			}

			if (block != null && relationToBlock != null)
			{
				relationToBlock.TagId = tag.Id;

				await db.BlockTags
					.Value(x => x.TagId, relationToBlock.TagId)
					.Value(x => x.BlockId, relationToBlock.BlockId)
					.Value(x => x.Name, relationToBlock.Name)
					.Value(x => x.Relation, relationToBlock.Relation)
					.InsertAsync();
			}

			await LogAsync(db, userGuid, tag.Id, $"Создан тег \"{createRequest.Name}\"");

			await transaction.CommitAsync();

			// 6. Перестроение структур
			stateHolder.UpdateState(state => state with
			{
				Blocks = state.Blocks,
				Tags = state.Tags.Add(tag),
				BlockTags = relationToBlock != null
					? state.BlockTags.Add(relationToBlock)
					: state.BlockTags,
				Version = DateTime.UtcNow.Ticks,
			});

			// 7. Вернуть ответ
			Tag? sourceTag;
			Source? sourceTagSource;

			var createdTagInfo = new TagInfo
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
					from input in db.Tags.InnerJoin(x => x.Id == input_rel.InputTagId && !x.IsDeleted)
					from input_source in db.Sources.LeftJoin(x => x.Id == input.SourceId && !x.IsDeleted)
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
				SourceTag = (sourceTag = state.Tags.FirstOrDefault(x => x.Id == tag.SourceTagId && !x.IsDeleted)) == null ? null : new TagSimpleInfo
				{
					Id = sourceTag.Id,
					Frequency = sourceTag.Frequency,
					Guid = sourceTag.GlobalGuid,
					Name = sourceTag.Name,
					Type = sourceTag.Type,
					SourceType = (sourceTagSource = state.Sources.FirstOrDefault(x => x.Id == sourceTag.SourceId && !x.IsDeleted)) == null ? SourceType.NotSet : sourceTagSource.Type,
				},
				Aggregation = tag.Aggregation,
				AggregationPeriod = tag.AggregationPeriod,
			};

			return createdTagInfo;
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new Exception("Не удалось создать тег", ex);
		}
	}

	internal async Task UpdateAsync(
		DatalakeContext db,
		Guid userGuid,
		Guid guid,
		TagUpdateRequest updateRequest)
	{
		updateRequest.Name = updateRequest.Name.RemoveWhitespaces("_");

		var state = stateHolder.CurrentState;

		var tag = state.Tags.FirstOrDefault(x => x.GlobalGuid == guid && !x.IsDeleted)
			?? throw new NotFoundException($"тег {guid}");

		var updatedTag = tag with
		{
			Name = updateRequest.Name,
			Description = updateRequest.Description,
			Type = updateRequest.Type,
			Frequency = updateRequest.Frequency,
			SourceId = updateRequest.SourceId,
			SourceItem = updateRequest.SourceItem,
			IsScaling = updateRequest.IsScaling,
			MaxEu = updateRequest.MaxEu,
			MinEu = updateRequest.MinEu,
			MaxRaw = updateRequest.MaxRaw,
			MinRaw = updateRequest.MinRaw,
			Formula = updateRequest.Formula,
			SourceTagId = updateRequest.SourceTagId,
			Aggregation = updateRequest.Aggregation,
			AggregationPeriod = updateRequest.AggregationPeriod,
		};

		if (state.Tags.Any(x => x.GlobalGuid != guid && x.Name == updateRequest.Name))
			throw new AlreadyExistException($"тег с именем {updateRequest.Name}");

		if (updateRequest.SourceId > 0)
		{
			if (string.IsNullOrEmpty(updateRequest.SourceItem))
				throw new InvalidValueException("Для несистемного источника обязателен путь к значению");

			updateRequest.SourceItem = updateRequest.SourceItem.RemoveWhitespaces();
		}
		else
		{
			updateRequest.SourceItem = null;
		}

		if (updateRequest.SourceTagId == tag.Id)
			throw new InvalidValueException("Тег не может быть источником значений для самого себя");

		List<string> changes = new();
		if (tag.Name != updateRequest.Name)
			changes.Add($"название: [{tag.Name}] > [{updateRequest.Name}]");
		if (tag.Description != updateRequest.Description)
			changes.Add($"описание: [{tag.Description}] > [{updateRequest.Description}]");
		if (tag.Type != updateRequest.Type)
			changes.Add($"тип значения: [{tag.Type}] > [{updateRequest.Type}]");
		if (tag.Frequency != updateRequest.Frequency)
			changes.Add($"частота: [{tag.Frequency}] > [{updateRequest.Frequency}]");
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
			.Select(x => new TagInput
			{
				TagId = tag.Id,
				InputTagId = x.TagId,
				VariableName = x.VariableName,
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

		var transaction = await db.BeginTransactionAsync();

		try
		{
			var createdTagBag = await db.Tags
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
				.Set(x => x.SourceTagId, updateRequest.SourceTagId)
				.Set(x => x.Aggregation, updateRequest.Aggregation)
				.Set(x => x.AggregationPeriod, updateRequest.AggregationPeriod)
				.UpdateWithOutputAsync();

			if (createdTagBag.Length != 1)
				throw new DatabaseException($"Не удалось сохранить тег {guid}", DatabaseStandartError.UpdatedZero);

			await db.TagInputs
				.Where(x => x.TagId == tag.Id)
				.DeleteAsync();

			await db.TagInputs.BulkCopyAsync(inputs);

			await LogAsync(db, userGuid, tag.Id, $"Изменен тег \"{tag.Name}\"", string.Join(",\n", changes));

			await transaction.CommitAsync();

			stateHolder.UpdateState(state => state with
			{
				Tags = state.Tags.Replace(tag, updatedTag),
				TagInputs = state.TagInputs.RemoveAll(x => x.TagId == tag.Id).AddRange(inputs),
			});
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new Exception("Не удалось обновить тег", ex);
		}
	}

	internal async Task DeleteAsync(
		DatalakeContext db, Guid userGuid, Guid guid)
	{
		var state = stateHolder.CurrentState;

		var tag = state.Tags.FirstOrDefault(x => x.GlobalGuid == guid && !x.IsDeleted)
			?? throw new NotFoundException($"тег {guid}");

		using var transaction = await db.BeginTransactionAsync();

		try
		{
			var count = await db.Tags
				.Where(x => x.GlobalGuid == guid)
				.Set(x => x.IsDeleted, true)
				.UpdateAsync();

			if (count == 0)
				throw new DatabaseException($"Не удалось удалить тег {tag.Name}", DatabaseStandartError.DeletedZero);

			// TODO: удаление истории тега. Так как доступ идёт по id, получить её после пересоздания не получится
			// Либо нужно сделать отслеживание соответствий локальный и глобальных id, и при получении истории обогащать выборку предыдущей историей

			await LogAsync(db, userGuid, tag.Id, $"Удален тег \"{tag.Name}\"");

			await transaction.CommitAsync();

			var updatedTag = tag with { IsDeleted = true };

			stateHolder.UpdateState(state => state with
			{
				Tags = state.Tags.Remove(tag).Add(updatedTag),
			});
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new Exception("Не удалось удалить тег", ex);
		}
	}

	internal static async Task LogAsync(DatalakeContext db, Guid userGuid, int tagId, string message, string? details = null)
	{
		await db.InsertAsync(new Log
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
}
