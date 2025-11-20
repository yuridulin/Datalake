using Datalake.Domain.Entities;
using Datalake.Domain.Interfaces;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Models;
using System.Collections.Immutable;

namespace Datalake.Inventory.Infrastructure.InMemory.Inventory;

public record class InventoryCacheState : IInventoryState
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
	public static InventoryCacheState Empty
	{
		get
		{
			var state = new InventoryCacheState
			{
				AccessRules = [],
				Blocks = ImmutableDictionary<int, BlockMemoryDto>.Empty,
				BlockTags = [],
				Sources = ImmutableDictionary<int, SourceMemoryDto>.Empty,
				Tags = ImmutableDictionary<int, TagMemoryDto>.Empty,
				Users = ImmutableDictionary<Guid, UserMemoryDto>.Empty,
				UserGroups = ImmutableDictionary<Guid, UserGroupMemoryDto>.Empty,
				UserGroupRelations = [],
			};

			state.UpdateVersion();

			return state;
		}
	}

	/// <summary>
	/// Фабричный метод создания нового состояния с нуля
	/// </summary>
	public static InventoryCacheState Create(
		IEnumerable<AccessRule> accessRules,
		IEnumerable<Block> blocks,
		IEnumerable<BlockTag> blockTags,
		IEnumerable<Source> sources,
		IEnumerable<Tag> tags,
		IEnumerable<User> users,
		IEnumerable<UserGroup> userGroups,
		IEnumerable<UserGroupRelation> userGroupRelations)
	{
		var state = new InventoryCacheState
		{
			AccessRules = accessRules.Select(AccessRightsMemoryDto.FromEntity).ToImmutableList(),

			Blocks = blocks.Select(BlockMemoryDto.FromEntity).ToImmutableDictionary(x => x.Id),
			Sources = sources.Select(SourceMemoryDto.FromEntity).ToImmutableDictionary(x => x.Id),
			Tags = tags.Select(TagMemoryDto.FromEntity).ToImmutableDictionary(x => x.Id),
			Users = users.Select(UserMemoryDto.FromEntity).ToImmutableDictionary(x => x.Guid),
			UserGroups = userGroups.Select(UserGroupMemoryDto.FromEntity).ToImmutableDictionary(x => x.Guid),

			BlockTags = blockTags.Select(BlockTagMemoryDto.FromEntity).ToImmutableList(),
			UserGroupRelations = userGroupRelations.Select(UserGroupRelationMemoryDto.FromEntity).ToImmutableList(),
		};

		state.UpdateVersion();

		return state;
	}

	#endregion Фабричные методы создания

	#region Коллекции

	public ImmutableDictionary<int, BlockMemoryDto> Blocks { get; private set; } = ImmutableDictionary<int, BlockMemoryDto>.Empty;

	public ImmutableDictionary<int, SourceMemoryDto> Sources { get; private set; } = ImmutableDictionary<int, SourceMemoryDto>.Empty;

	public ImmutableDictionary<int, TagMemoryDto> Tags { get; private set; } = ImmutableDictionary<int, TagMemoryDto>.Empty;

	public ImmutableDictionary<Guid, UserMemoryDto> Users { get; private set; } = ImmutableDictionary<Guid, UserMemoryDto>.Empty;

	public ImmutableDictionary<Guid, UserGroupMemoryDto> UserGroups { get; private set; } = ImmutableDictionary<Guid, UserGroupMemoryDto>.Empty;

	public required ImmutableList<AccessRightsMemoryDto> AccessRules { get; init; }

	public required ImmutableList<BlockTagMemoryDto> BlockTags { get; init; }

	public required ImmutableList<UserGroupRelationMemoryDto> UserGroupRelations { get; init; }

	#endregion Коллекции без ключей

	#region Вспомогательные методы для модификации

	public IInventoryState WithBlock(Block block)
	{
		return Update(state => state with
		{
			Blocks = Apply(Blocks, BlockMemoryDto.FromEntity(block)),
		});
	}

	public IInventoryState WithSource(Source source)
	{
		return Update(state => state with
		{
			Sources = Apply(Sources, SourceMemoryDto.FromEntity(source)),
		});
	}

	public IInventoryState WithTag(Tag tag)
	{
		return Update(state => state with
		{
			Tags = Apply(Tags, TagMemoryDto.FromEntity(tag)),
		});
	}

	public IInventoryState WithUser(User user)
	{
		return Update(state => state with
		{
			Users = Apply(Users, UserMemoryDto.FromEntity(user)),
		});
	}

	public IInventoryState WithUserGroup(UserGroup userGroup)
	{
		return Update(state => state with
		{
			UserGroups = Apply(UserGroups, UserGroupMemoryDto.FromEntity(userGroup)),
		});
	}

	public IInventoryState WithBlockTags(int blockId, IEnumerable<BlockTag> blockTags)
	{
		return this with
		{
			BlockTags = BlockTags.RemoveAll(x => x.BlockId == blockId).AddRange(blockTags.Select(BlockTagMemoryDto.FromEntity)),
		};
	}

	public IInventoryState WithTagBlocks(int tagId, IEnumerable<BlockTag> blockTags)
	{
		return this with
		{
			BlockTags = BlockTags.RemoveAll(x => x.TagId == tagId).AddRange(blockTags.Select(BlockTagMemoryDto.FromEntity)),
		};
	}

	public IInventoryState WithUserGroupRelations(Guid userGroupGuid, IEnumerable<UserGroupRelation> userGroupRelations)
	{
		return this with
		{
			UserGroupRelations = UserGroupRelations.RemoveAll(x => x.UserGroupGuid == userGroupGuid).AddRange(userGroupRelations.Select(UserGroupRelationMemoryDto.FromEntity)),
		};
	}

	public IInventoryState WithAccessRules(int[] oldRulesId, AccessRule[] newRules)
	{
		return this with
		{
			AccessRules = AccessRules
				.RemoveAll(x => oldRulesId.Contains(x.Id))
				.AddRange(newRules.Select(AccessRightsMemoryDto.FromEntity))
		};
	}

	#endregion Вспомогательные методы для модификации

	#region Внутренние методы-хэлперы

	private InventoryCacheState Update(Func<InventoryCacheState, InventoryCacheState> updateFunc)
	{
		InventoryCacheState newState = updateFunc(this);
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

	#endregion Внутренние методы-хэлперы
}