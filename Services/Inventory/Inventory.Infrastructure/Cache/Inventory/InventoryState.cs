using Datalake.Domain.Entities;
using Datalake.Domain.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using System.Collections.Immutable;

namespace Datalake.Inventory.Infrastructure.Cache.Inventory;

public record class InventoryState : IInventoryCacheState
{
	public long Version { get; private set; } = DateTime.UtcNow.Ticks;

	private void UpdateVersion()
	{
		Version = DateTime.UtcNow.Ticks;
	}

	#region Фабричные методы создания

	/// <summary>
	/// Фабричный метод пустого нового состояния
	/// </summary>
	public static InventoryState Empty
	{
		get
		{
			var state = new InventoryState
			{
				AccessRules = [],
				Blocks = ImmutableDictionary<int, BlockEntity>.Empty,
				BlockProperties = [],
				BlockTags = [],
				Sources = ImmutableDictionary<int, SourceEntity>.Empty,
				Tags = ImmutableDictionary<int, TagEntity>.Empty,
				TagInputs = [],
				TagThresholds = [],
				Users = ImmutableDictionary<Guid, UserEntity>.Empty,
				UserGroups = ImmutableDictionary<Guid, UserGroupEntity>.Empty,
				UserGroupRelations = [],
			};

			state.UpdateVersion();

			return state;
		}
	}

	/// <summary>
	/// Фабричный метод создания нового состояния с нуля
	/// </summary>
	public static InventoryState Create(
		IEnumerable<AccessRuleEntity> accessRules,
		IEnumerable<BlockEntity> blocks,
		IEnumerable<BlockPropertyEntity> blockProperties,
		IEnumerable<BlockTagEntity> blockTags,
		IEnumerable<SourceEntity> sources,
		IEnumerable<TagEntity> tags,
		IEnumerable<TagInputEntity> tagInputs,
		//IEnumerable<TagThresholdEntity> tagThresholds,
		IEnumerable<UserEntity> users,
		IEnumerable<UserGroupEntity> userGroups,
		IEnumerable<UserGroupRelationEntity> userGroupRelations)
	{
		var state = new InventoryState
		{
			AccessRules = accessRules.ToImmutableList(),
			Blocks = blocks.ToImmutableDictionary(x => x.Id),
			BlockProperties = blockProperties.ToImmutableList(),
			BlockTags = blockTags.ToImmutableList(),
			Sources = sources.ToImmutableDictionary(x => x.Id),
			Tags = tags.ToImmutableDictionary(x => x.Id),
			TagInputs = tagInputs.ToImmutableList(),
			TagThresholds = [] /*tagThresholds.ToImmutableList()*/,
			Users = users.ToImmutableDictionary(x => x.Guid),
			UserGroups = userGroups.ToImmutableDictionary(x => x.Guid),
			UserGroupRelations = userGroupRelations.ToImmutableList(),
		};

		state.SetActiveDictionaries();
		state.UpdateVersion();

		return state;
	}

	#endregion Фабричные методы создания

	#region Коллекции с первичными ключами

	public ImmutableDictionary<int, BlockEntity> Blocks { get; private set; } = ImmutableDictionary<int, BlockEntity>.Empty;

	public ImmutableDictionary<int, SourceEntity> Sources { get; private set; } = ImmutableDictionary<int, SourceEntity>.Empty;

	public ImmutableDictionary<int, TagEntity> Tags { get; private set; } = ImmutableDictionary<int, TagEntity>.Empty;

	public ImmutableDictionary<Guid, UserEntity> Users { get; private set; } = ImmutableDictionary<Guid, UserEntity>.Empty;

	public ImmutableDictionary<Guid, UserGroupEntity> UserGroups { get; private set; } = ImmutableDictionary<Guid, UserGroupEntity>.Empty;

	#endregion Коллекции с первичными ключами

	#region Коллекции без ключей

	public required ImmutableList<AccessRuleEntity> AccessRules { get; init; }

	public required ImmutableList<BlockPropertyEntity> BlockProperties { get; init; }

	public required ImmutableList<BlockTagEntity> BlockTags { get; init; }

	public required ImmutableList<TagInputEntity> TagInputs { get; init; }

	public required ImmutableList<TagThresholdEntity> TagThresholds { get; init; }

	public required ImmutableList<UserGroupRelationEntity> UserGroupRelations { get; init; }

	#endregion Коллекции без ключей

	#region Словари активных объектов (только не удаленные)

	private void SetActiveDictionaries()
	{
		// Активные объекты фильтруются из основных словарей
		ActiveBlocksById = Blocks.Values.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Id);
		ActiveSourcesById = Sources.Values.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Id);
		ActiveTagsByGuid = Tags.Values.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Guid);
		ActiveUsersByGuid = Users.Values.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Guid);
		ActiveUserGroupsByGuid = UserGroups.Values.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Guid);

		// Дополнительные индексы для тегов
		ActiveTagsById = ActiveTagsByGuid.Values.ToImmutableDictionary(x => x.Id);
	}

	public ImmutableDictionary<int, BlockEntity> ActiveBlocksById { get; private set; } = ImmutableDictionary<int, BlockEntity>.Empty;

	public ImmutableDictionary<int, SourceEntity> ActiveSourcesById { get; private set; } = ImmutableDictionary<int, SourceEntity>.Empty;

	public ImmutableDictionary<Guid, TagEntity> ActiveTagsByGuid { get; private set; } = ImmutableDictionary<Guid, TagEntity>.Empty;

	public ImmutableDictionary<int, TagEntity> ActiveTagsById { get; private set; } = ImmutableDictionary<int, TagEntity>.Empty;

	public ImmutableDictionary<Guid, UserEntity> ActiveUsersByGuid { get; private set; } = ImmutableDictionary<Guid, UserEntity>.Empty;

	public ImmutableDictionary<Guid, UserGroupEntity> ActiveUserGroupsByGuid { get; private set; } = ImmutableDictionary<Guid, UserGroupEntity>.Empty;

	#endregion Словари активных объектов (только не удаленные)

	#region Коллекции активных объектов (только не удаленные)

	public IEnumerable<BlockEntity> ActiveBlocks => ActiveBlocksById.Values;

	public IEnumerable<SourceEntity> ActiveSources => ActiveSourcesById.Values;

	public IEnumerable<TagEntity> ActiveTags => ActiveTagsById.Values;

	public IEnumerable<UserEntity> ActiveUsers => ActiveUsersByGuid.Values;

	public IEnumerable<UserGroupEntity> ActiveUserGroups => ActiveUserGroupsByGuid.Values;

	#endregion Коллекции активных объектов (только не удаленные)

	#region Вспомогательные методы для модификации

	public IInventoryCacheState WithBlock(BlockEntity block)
	{
		return Update(state => state with
		{
			Blocks = Apply(Blocks, block),
			ActiveBlocksById = ApplyAsActive(ActiveBlocksById, block),
		});
	}

	public IInventoryCacheState WithSource(SourceEntity source)
	{
		return Update(state => state with
		{
			Sources = Apply(Sources, source),
			ActiveSourcesById = ApplyAsActive(ActiveSourcesById, source),
		});
	}

	public IInventoryCacheState WithTag(TagEntity tag)
	{
		return Update(state => state with
		{
			Tags = Apply(Tags, tag),
			ActiveTagsById = ApplyAsActive(ActiveTagsById, tag),
			ActiveTagsByGuid = ApplyAsActive(ActiveTagsByGuid, tag),
		});
	}

	public IInventoryCacheState WithUser(UserEntity user)
	{
		return Update(state => state with
		{
			Users = Apply(Users, user),
			ActiveUsersByGuid = ApplyAsActive(ActiveUsersByGuid, user),
		});
	}

	public IInventoryCacheState WithUserGroup(UserGroupEntity userGroup)
	{
		return Update(state => state with
		{
			UserGroups = Apply(UserGroups, userGroup),
			ActiveUserGroupsByGuid = ApplyAsActive(ActiveUserGroupsByGuid, userGroup),
		});
	}

	public IInventoryCacheState WithBlockTags(int blockId, IEnumerable<BlockTagEntity> blockTags)
	{
		return this with
		{
			BlockTags = BlockTags.RemoveAll(x => x.BlockId == blockId).AddRange(blockTags),
		};
	}

	public IInventoryCacheState WithTagBlocks(int tagId, IEnumerable<BlockTagEntity> blockTags)
	{
		return this with
		{
			BlockTags = BlockTags.RemoveAll(x => x.TagId == tagId).AddRange(blockTags),
		};
	}

	public IInventoryCacheState WithTagInputs(int tagId, IEnumerable<TagInputEntity> tagInputs)
	{
		return this with
		{
			TagInputs = TagInputs.RemoveAll(x => x.TagId == tagId).AddRange(tagInputs),
		};
	}

	public IInventoryCacheState WithTagThresholds(int tagId, IEnumerable<TagThresholdEntity> tagThresholds)
	{
		return this/* with
		{
			TagThresholds = TagThresholds.RemoveAll(x => x.TagId == tagId).AddRange(tagThresholds),
		}*/;
	}

	public IInventoryCacheState WithUserGroupRelations(Guid userGroupGuid, IEnumerable<UserGroupRelationEntity> userGroupRelations)
	{
		return this with
		{
			UserGroupRelations = UserGroupRelations.RemoveAll(x => x.UserGroupGuid == userGroupGuid).AddRange(userGroupRelations),
		};
	}

	public IInventoryCacheState WithAccessRules(int[] oldRulesId, AccessRuleEntity[] newRules)
	{
		return this with
		{
			AccessRules = AccessRules
				.RemoveAll(x => oldRulesId.Contains(x.Id))
				.AddRange(newRules)
		};
	}

	#endregion

	#region Внутренние методы-хэлперы

	private InventoryState Update(Func<InventoryState, InventoryState> updateFunc)
	{
		InventoryState newState = updateFunc(this);
		newState.UpdateVersion();
		return newState;
	}

	private static ImmutableDictionary<int, TEntity> Apply<TEntity>(ImmutableDictionary<int, TEntity> dict, TEntity entity)
		where TEntity : IWithIdentityKey
	{
		return dict.ContainsKey(entity.Id) ? dict.SetItem(entity.Id, entity) : dict.Add(entity.Id, entity);
	}

	private static ImmutableDictionary<Guid, TEntity> Apply<TEntity>(ImmutableDictionary<Guid, TEntity> dict, TEntity entity)
		where TEntity : IWithGuidKey
	{
		return dict.ContainsKey(entity.Guid) ? dict.SetItem(entity.Guid, entity) : dict.Add(entity.Guid, entity);
	}

	private static ImmutableDictionary<int, TEntity> ApplyAsActive<TEntity>(ImmutableDictionary<int, TEntity> dict, TEntity entity)
		where TEntity : ISoftDeletable, IWithIdentityKey
	{
		if (entity.IsDeleted)
		{
			return dict.ContainsKey(entity.Id) ? dict.Remove(entity.Id) : dict;
		}
		else
		{
			return dict.ContainsKey(entity.Id) ? dict.SetItem(entity.Id, entity) : dict.Add(entity.Id, entity);
		}
	}

	private static ImmutableDictionary<Guid, TEntity> ApplyAsActive<TEntity>(ImmutableDictionary<Guid, TEntity> dict, TEntity entity)
		where TEntity : ISoftDeletable, IWithGuidKey
	{
		if (entity.IsDeleted)
		{
			return dict.ContainsKey(entity.Guid) ? dict.Remove(entity.Guid) : dict;
		}
		else
		{
			return dict.ContainsKey(entity.Guid) ? dict.SetItem(entity.Guid, entity) : dict.Add(entity.Guid, entity);
		}
	}

	#endregion Внутренние методы-хэлперы
}