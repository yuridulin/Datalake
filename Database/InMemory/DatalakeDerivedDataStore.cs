using Datalake.Database.Functions;
using Datalake.Database.InMemory.Models;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Blocks;
using Microsoft.Extensions.Logging;

namespace Datalake.Database.InMemory;

/// <summary>
/// Хранилище производных данных
/// </summary>
public class DatalakeDerivedDataStore
{
	/// <summary>Конструктор</summary>
	public DatalakeDerivedDataStore(
		DatalakeDataStore dataStore,
		ILogger<DatalakeDerivedDataStore> logger)
	{
		_logger = logger;
		dataStore.StateChanged += (_, newState) =>
		{
			if (newState.Version > _lastProcessingVersion)
			{
				_lastProcessingVersion = newState.Version;

				Task.Run(() => Rebuild(newState));
			}
		};

		if (_lastProcessingVersion == -1)
			Task.Run(() => Rebuild(dataStore.State));
	}

	private void Rebuild(DatalakeDataState newState)
	{
		RebuildTree(newState);
		RebuildAccess(newState);

		_logger.LogInformation("Завершено обновление зависимых данных");
	}

	private long _lastProcessingVersion = -1;
	private readonly ILogger<DatalakeDerivedDataStore> _logger;

	#region Дерево блоков

	private BlockTreeInfo[] _cachedBlockTree = [];

	private void RebuildTree(DatalakeDataState state)
	{
		var nextBlockTree = CreateBlocksTree(state);
		Interlocked.Exchange(ref _cachedBlockTree, nextBlockTree);
	}

	/// <summary>
	/// Расчет дерева блоков
	/// </summary>
	/// <param name="state">Текущее состояние данных</param>
	/// <returns>Новое дерево блоков</returns>
	private static BlockTreeInfo[] CreateBlocksTree(DatalakeDataState state)
	{
		var tagsDict = state.Tags.ToDictionary(x => x.Id);

		var blocksWithTags = state.Blocks
			.Select(block => new BlockWithTagsInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				ParentId = block.ParentId,
				Description = block.Description,
				Name = block.Name,
				Tags = state.BlockTags
					.Where(x => x.BlockId == block.Id)
					.Select(x =>
					{
						if (x.TagId.HasValue && tagsDict.TryGetValue(x.TagId.Value, out var tag))
						{
							return new BlockNestedTagInfo
							{
								Id = tag.Id,
								Guid = tag.GlobalGuid,
								Name = tag.Name,
								Frequency = tag.Frequency,
								Type = tag.Type,
								SourceType = SourceType.NotSet,
								LocalName = x.Name ?? string.Empty,
								Relation = x.Relation,
								SourceId = tag.SourceId,
							};
						}
						else
							return null!;
					})
					.Where(x => x != null)
					.ToArray(),
			})
			.ToArray();

		return ReadBlockChildren(blocksWithTags, null, string.Empty);

		BlockTreeInfo[] ReadBlockChildren(BlockWithTagsInfo[] blocks, int? id, string prefix)
		{
			var prefixString = prefix + (string.IsNullOrEmpty(prefix) ? string.Empty : ".");
			return blocks
				.Where(x => x.ParentId == id)
				.Select(x =>
				{
					var block = new BlockTreeInfo
					{
						Id = x.Id,
						Guid = x.Guid,
						ParentId = x.ParentId,
						Name = x.Name,
						FullName = prefixString + x.Name,
						Description = x.Description,
						Tags = x.Tags
							.Select(tag => new BlockNestedTagInfo
							{
								Guid = tag.Guid,
								Name = tag.Name,
								Id = tag.Id,
								Relation = tag.Relation,
								SourceId = tag.SourceId,
								LocalName = tag.LocalName,
								Type = tag.Type,
								Frequency = tag.Frequency,
								SourceType = tag.SourceType,
							})
							.ToArray(),
						AccessRule = x.AccessRule,
						Children = ReadBlockChildren(blocks, x.Id, prefixString + x.Name),
					};
					return block;
				})
				.OrderBy(x => x.Name)
				.ToArray();
		}
	}

	/// <summary>
	/// Получение дерева блоков со списком полей каждого блока
	/// </summary>
	/// <returns>Коллекция корневых элементом дерева</returns>
	public BlockTreeInfo[] BlocksTree => _cachedBlockTree;

	#endregion

	#region Права пользователей

	private DatalakeAccessState _accessState = new();

	private void RebuildAccess(DatalakeDataState state)
	{
		var accessState = AccessFunctions.ComputeAccess(state);
		Interlocked.Exchange(ref _accessState, accessState);
		AccessChanged?.Invoke(this, accessState);
	}

	/// <summary>
	/// Разрешения пользователей, рассчитанные на каждый объект системы
	/// </summary>
	/// <returns>Разрешения, сгруппированные по идентификатору пользователя</returns>
	public DatalakeAccessState Access => _accessState;

	/// <summary>
	/// Событие при изменении разрешений пользователей
	/// </summary>
	public event EventHandler<DatalakeAccessState>? AccessChanged;

	#endregion
}
