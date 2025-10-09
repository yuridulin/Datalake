using Datalake.Domain.Entities;
using Datalake.Domain.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using System.Collections.Immutable;

namespace Datalake.Inventory.Infrastructure.InMemory.Inventory;

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
				Blocks = ImmutableDictionary<int, Block>.Empty,
				BlockProperties = [],
				BlockTags = [],
				Sources = ImmutableDictionary<int, Source>.Empty,
				Tags = ImmutableDictionary<int, Tag>.Empty,
				TagInputs = [],
				TagThresholds = [],
				Users = ImmutableDictionary<Guid, User>.Empty,
				UserGroups = ImmutableDictionary<Guid, UserGroup>.Empty,
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
		IEnumerable<AccessRights> accessRules,
		IEnumerable<Block> blocks,
		IEnumerable<BlockProperty> blockProperties,
		IEnumerable<BlockTag> blockTags,
		IEnumerable<Source> sources,
		IEnumerable<Tag> tags,
		IEnumerable<TagInput> tagInputs,
		//IEnumerable<TagThresholdEntity> tagThresholds,
		IEnumerable<User> users,
		IEnumerable<UserGroup> userGroups,
		IEnumerable<UserGroupRelation> userGroupRelations)
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

	public ImmutableDictionary<int, Block> Blocks { get; private set; } = ImmutableDictionary<int, Block>.Empty;

	public ImmutableDictionary<int, Source> Sources { get; private set; } = ImmutableDictionary<int, Source>.Empty;

	public ImmutableDictionary<int, Tag> Tags { get; private set; } = ImmutableDictionary<int, Tag>.Empty;

	public ImmutableDictionary<Guid, User> Users { get; private set; } = ImmutableDictionary<Guid, User>.Empty;

	public ImmutableDictionary<Guid, UserGroup> UserGroups { get; private set; } = ImmutableDictionary<Guid, UserGroup>.Empty;

	#endregion Коллекции с первичными ключами

	#region Коллекции без ключей

	public required ImmutableList<AccessRights> AccessRules { get; init; }

	public required ImmutableList<BlockProperty> BlockProperties { get; init; }

	public required ImmutableList<BlockTag> BlockTags { get; init; }

	public required ImmutableList<TagInput> TagInputs { get; init; }

	public required ImmutableList<TagThreshold> TagThresholds { get; init; }

	public required ImmutableList<UserGroupRelation> UserGroupRelations { get; init; }

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

	public ImmutableDictionary<int, Block> ActiveBlocksById { get; private set; } = ImmutableDictionary<int, Block>.Empty;

	public ImmutableDictionary<int, Source> ActiveSourcesById { get; private set; } = ImmutableDictionary<int, Source>.Empty;

	public ImmutableDictionary<Guid, Tag> ActiveTagsByGuid { get; private set; } = ImmutableDictionary<Guid, Tag>.Empty;

	public ImmutableDictionary<int, Tag> ActiveTagsById { get; private set; } = ImmutableDictionary<int, Tag>.Empty;

	public ImmutableDictionary<Guid, User> ActiveUsersByGuid { get; private set; } = ImmutableDictionary<Guid, User>.Empty;

	public ImmutableDictionary<Guid, UserGroup> ActiveUserGroupsByGuid { get; private set; } = ImmutableDictionary<Guid, UserGroup>.Empty;

	#endregion Словари активных объектов (только не удаленные)

	#region Коллекции активных объектов (только не удаленные)

	public IEnumerable<Block> ActiveBlocks => ActiveBlocksById.Values;

	public IEnumerable<Source> ActiveSources => ActiveSourcesById.Values;

	public IEnumerable<Tag> ActiveTags => ActiveTagsById.Values;

	public IEnumerable<User> ActiveUsers => ActiveUsersByGuid.Values;

	public IEnumerable<UserGroup> ActiveUserGroups => ActiveUserGroupsByGuid.Values;

	#endregion Коллекции активных объектов (только не удаленные)

	#region Вспомогательные методы для модификации

	public IInventoryCacheState WithBlock(Block block)
	{
		return Update(state => state with
		{
			Blocks = Apply(Blocks, block),
			ActiveBlocksById = ApplyAsActive(ActiveBlocksById, block),
		});
	}

	public IInventoryCacheState WithSource(Source source)
	{
		return Update(state => state with
		{
			Sources = Apply(Sources, source),
			ActiveSourcesById = ApplyAsActive(ActiveSourcesById, source),
		});
	}

	public IInventoryCacheState WithTag(Tag tag)
	{
		return Update(state => state with
		{
			Tags = Apply(Tags, tag),
			ActiveTagsById = ApplyAsActive(ActiveTagsById, tag),
			ActiveTagsByGuid = ApplyAsActive(ActiveTagsByGuid, tag),
		});
	}

	public IInventoryCacheState WithUser(User user)
	{
		return Update(state => state with
		{
			Users = Apply(Users, user),
			ActiveUsersByGuid = ApplyAsActive(ActiveUsersByGuid, user),
		});
	}

	public IInventoryCacheState WithUserGroup(UserGroup userGroup)
	{
		return Update(state => state with
		{
			UserGroups = Apply(UserGroups, userGroup),
			ActiveUserGroupsByGuid = ApplyAsActive(ActiveUserGroupsByGuid, userGroup),
		});
	}

	public IInventoryCacheState WithBlockTags(int blockId, IEnumerable<BlockTag> blockTags)
	{
		return this with
		{
			BlockTags = BlockTags.RemoveAll(x => x.BlockId == blockId).AddRange(blockTags),
		};
	}

	public IInventoryCacheState WithTagBlocks(int tagId, IEnumerable<BlockTag> blockTags)
	{
		return this with
		{
			BlockTags = BlockTags.RemoveAll(x => x.TagId == tagId).AddRange(blockTags),
		};
	}

	public IInventoryCacheState WithTagInputs(int tagId, IEnumerable<TagInput> tagInputs)
	{
		return this with
		{
			TagInputs = TagInputs.RemoveAll(x => x.TagId == tagId).AddRange(tagInputs),
		};
	}

	public IInventoryCacheState WithTagThresholds(int tagId, IEnumerable<TagThreshold> tagThresholds)
	{
		return this/* with
		{
			TagThresholds = TagThresholds.RemoveAll(x => x.TagId == tagId).AddRange(tagThresholds),
		}*/;
	}

	public IInventoryCacheState WithUserGroupRelations(Guid userGroupGuid, IEnumerable<UserGroupRelation> userGroupRelations)
	{
		return this with
		{
			UserGroupRelations = UserGroupRelations.RemoveAll(x => x.UserGroupGuid == userGroupGuid).AddRange(userGroupRelations),
		};
	}

	public IInventoryCacheState WithAccessRules(int[] oldRulesId, AccessRights[] newRules)
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