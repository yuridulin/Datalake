using Datalake.Database.Extensions;
using Datalake.Database.Repositories;
using Datalake.Database.Tables;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Tags;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory;

/// <summary>
/// 
/// </summary>
public class TagsMemoryRepository
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<int, Tag> _tags = [];

	internal IReadOnlyTag[] Tags => _tags.Values.Select(x => (IReadOnlyTag)x).ToArray();

	internal IReadOnlyDictionary<int, IReadOnlyTag> TagsDict => Tags.ToDictionary(x => x.Id);

	#endregion


	#region Версия данных для синхронизации

	private string _globalVersion = string.Empty;
	private readonly object _versionLock = new();

	/// <summary>
	/// Событие изменения списка блоков
	/// </summary>
	public event EventHandler<int>? TagsUpdated;

	#endregion


	#region Инициализация

	/// <summary>
	/// Конструктор репозитория
	/// </summary>
	public TagsMemoryRepository(
		Lazy<BlocksMemoryRepository> blocksRepository,
		Lazy<SourcesMemoryRepository> sourcesRepository,
		IServiceScopeFactory serviceScopeFactory)
	{
		_blocksRepository = blocksRepository;
		_sourcesRepository = sourcesRepository;

		using var scope = serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		InitializeFromDatabase(db).Wait();
		TagsUpdated?.Invoke(this, 0);
	}

	private Lazy<BlocksMemoryRepository> _blocksRepository;
	private BlocksMemoryRepository blocksRepository => _blocksRepository.Value;

	private Lazy<SourcesMemoryRepository> _sourcesRepository;
	private SourcesMemoryRepository sourcesRepository => _sourcesRepository.Value;

	#endregion


	#region Чтение данных из БД

	private async Task InitializeFromDatabase(DatalakeContext db)
	{
		_tags.Clear();

		var tags = await db.Tags.ToArrayAsync();
		foreach (var tag in tags)
			_tags.TryAdd(tag.Id, tag);

		_globalVersion = DateTime.UtcNow.Ticks.ToString();
	}

	#endregion


	#region Чтение данных внешними источниками

	/// <summary>
	/// Получение текущей версии данных репозитория
	/// </summary>
	public string CurrentVersion
	{
		get { lock (_versionLock) return _globalVersion; }
	}

	/// <summary>
	/// Установка новой версии данных репозитория
	/// </summary>
	/// <param name="newVersion">Значение новой версии</param>
	public void UpdateVersion(string newVersion)
	{
		lock (_versionLock)
			_globalVersion = newVersion;
	}

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

		if (createRequest.SourceId.HasValue)
		{
			if (!string.IsNullOrEmpty(createRequest.SourceItem))
			{
				createRequest.SourceItem = createRequest.SourceItem.RemoveWhitespaces();
			}

			var source = await db.Sources
				.Where(x => x.Id == createRequest.SourceId && !x.IsDeleted)
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
			if (!blocksRepository.BlocksDict.TryGetValue(createRequest.BlockId.Value, out var block))
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
			TagsUpdated?.Invoke(this, 0);
			
			// 7. Вернуть ответ
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new Exception("Не удалось обновить блок", ex);
		}
	}

	internal static async Task UpdateAsync(
		DatalakeContext db,
		Guid userGuid,
		Guid guid, TagUpdateRequest updateRequest)
	{
		var transaction = await db.BeginTransactionAsync();

		updateRequest.Name = updateRequest.Name.RemoveWhitespaces("_");

		var tag = await TagsNotDeleted(db).Where(x => x.GlobalGuid == guid).FirstOrDefaultAsync()
			?? throw new NotFoundException($"тег {guid}");

		if (await TagsNotDeleted(db).AnyAsync(x => x.GlobalGuid != guid && x.Name == updateRequest.Name))
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

		if (updateRequest.SourceTagId == tag.Id)
			throw new InvalidValueException("Тег не может быть источником значений для самого себя");

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

		var inputs = await db.TagInputs
			.Where(x => x.TagId == tag.Id)
			.ToListAsync();

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

		await LogAsync(db, userGuid, tag.Id, $"Изменен тег \"{tag.Name}\"", string.Join(",\n", changes));

		await transaction.CommitAsync();

		await UpdateTagCache(db, tag.Id);
	}

	internal static async Task DeleteAsync(
		DatalakeContext db, Guid userGuid, Guid guid)
	{
		var transaction = await db.BeginTransactionAsync();

		var tag = await TagsNotDeleted(db).Where(x => x.GlobalGuid == guid).FirstOrDefaultAsync()
			?? throw new NotFoundException($"тег {guid}");

		var cached = CachedTags.Values.FirstOrDefault(x => x.Guid == guid)
			?? throw new NotFoundException(message: $"тег {guid}");

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

		await UpdateTagCache(db, cached.Id);

		AccessRepository.Update();
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
