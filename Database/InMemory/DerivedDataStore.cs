using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Blocks;

namespace Datalake.Database.InMemory;

/// <summary>
/// Хранилище производных данных
/// </summary>
public class DerivedDataStore(DatalakeStateHolder stateHolder)
{
	private long _lastProcessedVersion = -1;

	#region Дерево блоков

	/// <summary>
	/// Получение дерева блоков со списком полей каждого блока
	/// </summary>
	/// <returns>Коллекция корневых элементом дерева</returns>
	public BlockTreeInfo[] BlocksTree()
	{
		var currentState = stateHolder.CurrentState;

		// Если состояние не изменилось - возвращаем кэш
		if (currentState.Version == _lastProcessedVersion)
			return _cachedBlockTree;

		lock (this)
		{
			// Двойная проверка
			if (currentState.Version == _lastProcessedVersion)
				return _cachedBlockTree;

			// атомарная замена
			Interlocked.Exchange(ref _cachedBlockTree, RebuildTree(currentState));

			_lastProcessedVersion = currentState.Version;

			return _cachedBlockTree;
		}
	}

	private BlockTreeInfo[] _cachedBlockTree = null!;

	private static BlockTreeInfo[] RebuildTree(DatalakeState state)
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

		var nextBlockTree = ReadBlockChildren(blocksWithTags, null, string.Empty);

		return nextBlockTree;
	}

	private static BlockTreeInfo[] ReadBlockChildren(BlockWithTagsInfo[] blocks, int? id, string prefix)
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

				/*if (!x.AccessRule.AccessType.HasAccess(AccessType.Viewer))
				{
					block.Name = string.Empty;
					block.Description = string.Empty;
					block.Tags = [];
				}*/

				return block;
			})
			/*.Where(x => x.Children.Length > 0 || x.AccessRule.AccessType.HasAccess(AccessType.Viewer))*/
			.OrderBy(x => x.Name)
			.ToArray();
	}

	#endregion

	// другие зависимые структуры
}
