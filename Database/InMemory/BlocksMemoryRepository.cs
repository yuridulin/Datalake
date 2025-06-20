using Datalake.Database.Tables;
using Datalake.PublicApi.Models.Blocks;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory;

/// <summary>
/// Репозиторий работы с блоками в памяти приложения
/// </summary>
public class BlocksMemoryRepository
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<int, Block> _blocks = [];
	private readonly ConcurrentDictionary<int, Tag> _tags = [];
	private ConcurrentBag<BlockTag> _relationBlockTags = [];

	internal IReadOnlyBlock[] Blocks
		=> _blocks.Values.Select(x => (IReadOnlyBlock)x).ToArray();

	internal IReadOnlyTag[] Tags
		=> _tags.Values.Select(x => (IReadOnlyTag)x).ToArray();

	internal IReadOnlyDictionary<int, IReadOnlyBlock> BlocksDict
		=> _blocks.ToDictionary(x => x.Key, x => (IReadOnlyBlock)x.Value);

	internal IReadOnlyDictionary<int, IReadOnlyTag> TagsDict
		=> _tags.ToDictionary(x => x.Key, x => (IReadOnlyTag)x.Value);

	internal IReadOnlyCollection<IReadOnlyBlockTag> RelationsBlockTags
		=> _relationBlockTags.Select(x => (IReadOnlyBlockTag)x).ToArray();

	#endregion


	#region Версия данных для синхронизации

	private string _globalVersion = string.Empty;
	private readonly object _versionLock = new();

	/// <summary>
	/// Событие изменения списка блоков
	/// </summary>
	public event EventHandler<int>? BlocksUpdated;

	#endregion


	#region Инициализация

	/// <summary>
	/// Конструктор репозитория
	/// </summary>
	/// <param name="serviceScopeFactory">Фабрика сервисов</param>
	public BlocksMemoryRepository(IServiceScopeFactory serviceScopeFactory)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		InitializeFromDatabase(db).Wait();
		BlocksUpdated?.Invoke(this, 0);
	}

	#endregion


	#region Чтение данных из БД

	private async Task InitializeFromDatabase(DatalakeContext db)
	{
		_blocks.Clear();
		_tags.Clear();
		_relationBlockTags.Clear();

		var blocks = await db.Blocks.ToArrayAsync();
		foreach (var block in blocks)
			_blocks.TryAdd(block.Id, block);

		var tags = await db.Tags.ToArrayAsync();
		foreach (var tag in tags)
			_tags.TryAdd(tag.Id, tag);

		var relationsBlockTag = await db.BlockTags.ToArrayAsync();
		foreach (var rel in relationsBlockTag)
			_relationBlockTags.Add(rel);

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
		lock (_versionLock) _globalVersion = newVersion;
	}

	#endregion


	#region Изменение данных внешними источниками

	/// <summary>
	/// Изменение блока
	/// </summary>
	/// <param name="db">Контекст БД</param>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="request">Новые данные</param>
	public async Task UpdateBlock(DatalakeContext db, int id, BlockUpdateRequest request)
	{
		// 1. Проверка версии данных, на основе которых сделан запрос
		//if (request.LastKnownVersion != CurrentVersion)
			//throw new Exception("Данные не актуальны");

		if (!_blocks.TryGetValue(id, out var storedBlock))
			throw new Exception("Блок не найден по id");

		if (_blocks.Values.Any(x => x.Name == request.Name && x.Id != id))
			throw new Exception("Блок с таким именем уже существует");

		using var transaction = await db.BeginTransactionAsync();

		try
		{
			// 2. Обновление в БД
			int count = await db.Blocks
				.Where(x => x.Id == id)
				.Set(x => x.Name, request.Name)
				.Set(x => x.Description, request.Description)
				.UpdateAsync();

			// 3. Обновление связей
			var newTagsRelations = request.Tags
				.Select(x => new BlockTag
				{
					BlockId = id,
					TagId = x.Id,
					Name = x.Name,
					Relation = x.Relation,
				})
				.ToArray();

			await db.BlockTags
				.Where(x => x.BlockId == id)
				.DeleteAsync();

			if (newTagsRelations.Length > 0)
				await db.BlockTags.BulkCopyAsync(newTagsRelations);

			// 4. Обновление in-memory
			storedBlock.Name = request.Name;

			var nextRelationBlockTags = new ConcurrentBag<BlockTag>(_relationBlockTags
				.ToArray()
				.Where(x => x.BlockId != id)
				.Concat(newTagsRelations!));

			Interlocked.Exchange(ref _relationBlockTags, nextRelationBlockTags);

			// 5. Обновление глобальной версии
			var newVersion = DateTime.UtcNow.Ticks.ToString();
			UpdateVersion(newVersion);

			await transaction.CommitAsync();

			// 6. Перестроение структур (без блокировки основного потока)
			//await Task.Run(RebuildTree).ConfigureAwait(false);
			BlocksUpdated?.Invoke(this, 0);
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new Exception("Не удалось обновить блок", ex);
		}
	}

	#endregion
}
