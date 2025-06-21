using Datalake.Database.Extensions;
using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Tags;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory.Repositories;

/// <summary>
/// Репозиторий тегов
/// </summary>
public class TagsMemoryRepository(
	IServiceScopeFactory serviceScopeFactory,
	Lazy<InMemoryRepositoriesManager> inMemory) : InMemoryRepositoryBase(serviceScopeFactory, inMemory)
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<int, Tag> _tags = [];

	private readonly ConcurrentDictionary<Guid, Tag> _tagGuids = [];

	private ConcurrentBag<TagInput> _tagInputs = [];

	#endregion


	#region Инициализация

	/// <inheritdoc/>
	protected override async Task InitializeFromDatabase(DatalakeContext db)
	{
		_tags.Clear();

		var tags = await db.Tags.ToArrayAsync();
		foreach (var tag in tags)
		{
			_tags.TryAdd(tag.Id, tag);
			_tagGuids.TryAdd(tag.GlobalGuid, tag);
		}

		var tagInputs = await db.TagInputs.ToArrayAsync();
		foreach (var tagInput in tagInputs)
		{
			_tagInputs.Add(tagInput);
		}
	}

	#endregion


	#region Чтение данных внешними источниками

	internal IReadOnlyTag[] Tags => _tags.Values.Select(x => (IReadOnlyTag)x).ToArray();

	internal IReadOnlyDictionary<int, IReadOnlyTag> TagsDict => Tags.ToDictionary(x => x.Id);

	internal IReadOnlyTagInput[] TagInputs => _tagInputs.Select(x => (IReadOnlyTagInput)x).ToArray();

	#endregion


	#region Изменение данных внешними источниками

	internal async Task<TagInfo> CreateAsync(
		DatalakeContext db,
		Guid userGuid,
		TagCreateRequest createRequest)
	{
		// 1. Проверка версии данных, на основе которых сделан запрос

		if (!createRequest.SourceId.HasValue && !createRequest.BlockId.HasValue)
			throw new InvalidValueException(message: "тег не может быть создан без привязок, нужно указать или источник, или блок");

		bool needToAddIdInName = string.IsNullOrEmpty(createRequest.Name);
		if (!string.IsNullOrEmpty(createRequest.Name))
		{
			createRequest.Name = createRequest.Name.RemoveWhitespaces("_");

			if (_tags.Values.Any(x => !x.IsDeleted && x.Name.ToLower() == createRequest.Name.ToLower()))
				throw new ForbiddenException(message: "уже существует тег с таким именем");
		}

		IReadOnlySource? source = null;
		IReadOnlyBlock? block = null;
		if (createRequest.SourceId.HasValue)
		{
			if (!string.IsNullOrEmpty(createRequest.SourceItem))
			{
				createRequest.SourceItem = createRequest.SourceItem.RemoveWhitespaces();
			}

			if (!InMemory.Sources.SourcesDict.TryGetValue(createRequest.SourceId.Value, out source) || source.IsDeleted)
				throw new NotFoundException(message: $"источник #{createRequest.SourceId}");

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
			if (!InMemory.Blocks.BlocksDict.TryGetValue(createRequest.BlockId.Value, out block) || block.IsDeleted)
				throw new NotFoundException(message: $"блок #{createRequest.BlockId}");

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

			// 3. Обновление связей
			// 4. Обновление in-memory
			_tags.TryAdd(tag.Id, tag);

			await LogAsync(db, userGuid, tag.Id, $"Создан тег \"{createRequest.Name}\"");

			// 5. Обновление глобальной версии
			var newVersion = DateTime.UtcNow.Ticks.ToString();
			UpdateVersion(newVersion);

			await transaction.CommitAsync();

			// 6. Перестроение структур
			Trigger();

			// 7. Вернуть ответ
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
				SourceTag = !_tags.TryGetValue(tag.SourceTagId ?? 0, out var sourceTag) ? null : new TagSimpleInfo
				{
					Id = sourceTag.Id,
					Frequency = sourceTag.Frequency,
					Guid = sourceTag.GlobalGuid,
					Name = sourceTag.Name,
					Type = sourceTag.Type,
					SourceType = !InMemory.Sources.SourcesDict.TryGetValue(sourceTag.SourceId, out var sourceTagSource) ? SourceType.NotSet : sourceTagSource.Type,
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

		if (!_tagGuids.TryGetValue(guid, out var tag))
			throw new NotFoundException($"тег {guid}");

		if (_tags.Values.Any(x => x.GlobalGuid != guid && x.Name == updateRequest.Name))
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
				.Set(x => x.SourceTagId, updateRequest.SourceTagId)
				.Set(x => x.Aggregation, updateRequest.Aggregation)
				.Set(x => x.AggregationPeriod, updateRequest.AggregationPeriod)
				.UpdateAsync();

			if (count != 1)
				throw new DatabaseException($"Не удалось сохранить тег {guid}", DatabaseStandartError.UpdatedZero);

			await db.TagInputs
				.Where(x => x.TagId == tag.Id)
				.DeleteAsync();

			await db.TagInputs.BulkCopyAsync(inputs);

			tag.Name = updateRequest.Name;
			tag.Description = updateRequest.Description;
			tag.Type = updateRequest.Type;
			tag.Frequency = updateRequest.Frequency;
			tag.SourceId = updateRequest.SourceId;
			tag.SourceItem = updateRequest.SourceItem;
			tag.IsScaling = updateRequest.IsScaling;
			tag.MaxEu = updateRequest.MaxEu;
			tag.MinEu = updateRequest.MinEu;
			tag.MaxRaw = updateRequest.MaxRaw;
			tag.MinRaw = updateRequest.MinRaw;
			tag.Formula = updateRequest.Formula;
			tag.SourceTagId = updateRequest.SourceTagId;
			tag.Aggregation = updateRequest.Aggregation;
			tag.AggregationPeriod = updateRequest.AggregationPeriod;

			await LogAsync(db, userGuid, tag.Id, $"Изменен тег \"{tag.Name}\"", string.Join(",\n", changes));

			var _nextTagInputs = new ConcurrentBag<TagInput>(_tagInputs
				.ToArray()
				.Where(x => x.TagId != tag.Id)
				.Concat(inputs));

			Interlocked.Exchange(ref _tagInputs, _nextTagInputs);

			await transaction.CommitAsync();
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
		if (!_tagGuids.TryGetValue(guid, out var tag))
			throw new NotFoundException($"тег {guid}");

		int id = tag.Id;

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

			await LogAsync(db, userGuid, id, $"Удален тег \"{tag.Name}\"");

			tag.IsDeleted = true;

			await transaction.CommitAsync();
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

	#endregion
}
