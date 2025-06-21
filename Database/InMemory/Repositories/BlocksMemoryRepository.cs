using Datalake.Database.Interfaces;
using Datalake.Database.Tables;
using Datalake.PublicApi.Models.Blocks;
using LinqToDB;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Datalake.Database.InMemory.Repositories;

/// <summary>
/// Репозиторий работы с блоками в памяти приложения
/// </summary>
public class BlocksMemoryRepository(
	IServiceScopeFactory serviceScopeFactory,
	Lazy<InMemoryRepositoriesManager> inMemory) : InMemoryRepositoryBase(serviceScopeFactory, inMemory)
{
	#region Исходные коллекции

	private readonly ConcurrentDictionary<int, Block> _blocks = [];
	private ConcurrentBag<BlockTag> _relationBlockTags = [];

	#endregion


	#region Инициализация

	/// <inheritdoc/>
	protected override async Task InitializeFromDatabase(DatalakeContext db)
	{
		_blocks.Clear();
		_relationBlockTags.Clear();

		var blocks = await db.Blocks.ToArrayAsync();
		foreach (var block in blocks)
			_blocks.TryAdd(block.Id, block);

		var relationsBlockTag = await db.BlockTags.ToArrayAsync();
		foreach (var rel in relationsBlockTag)
			_relationBlockTags.Add(rel);
	}

	#endregion


	#region Чтение данных внешними источниками

	internal IReadOnlyBlock[] Blocks
		=> _blocks.Values.Select(x => (IReadOnlyBlock)x).ToArray();

	internal IReadOnlyDictionary<int, IReadOnlyBlock> BlocksDict
		=> _blocks.ToDictionary(x => x.Key, x => (IReadOnlyBlock)x.Value);

	internal IReadOnlyCollection<IReadOnlyBlockTag> RelationsBlockTags
		=> _relationBlockTags.Select(x => (IReadOnlyBlockTag)x).ToArray();

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

			// 6. Перестроение структур
			Trigger();
		}
		catch (Exception ex)
		{
			await transaction.RollbackAsync();
			throw new Exception("Не удалось обновить блок", ex);
		}
	}

	#endregion
}
