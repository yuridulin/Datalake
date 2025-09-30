using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Interfaces;
using System.Collections.Immutable;

namespace Datalake.InventoryService.Infrastructure.Cache.Inventory;

/// <summary>
/// Снимок данных БД
/// </summary>
public record class InventoryState
{
	/// <summary>
	/// Версия снимка
	/// </summary>
	public long Version { get; private set; } = DateTime.UtcNow.Ticks;

	private void UpdateVersion()
	{
		Version = DateTime.UtcNow.Ticks;
	}

	#region Фабричные методы создания

	/// <summary>
	/// Фабричный метод пустого нового состояния
	/// </summary>
	/// <returns></returns>
	public static InventoryState CreateEmpty()
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
			Users = ImmutableDictionary<Guid, UserEntity>.Empty,
			UserGroups = ImmutableDictionary<Guid, UserGroupEntity>.Empty,
			UserGroupRelations = [],
		};

		state.UpdateVersion();

		return state;
	}

	/// <summary>
	/// Фабричный метод создания нового состояния с нуля
	/// </summary>
	/// <returns></returns>
	public static InventoryState Create(
		IEnumerable<AccessRuleEntity> accessRules,
		IEnumerable<BlockEntity> blocks,
		IEnumerable<BlockPropertyEntity> blockProperties,
		IEnumerable<BlockTagEntity> blockTags,
		IEnumerable<SourceEntity> sources,
		IEnumerable<TagEntity> tags,
		IEnumerable<TagInputEntity> tagInputs,
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

	/// <summary>
	/// Блоки по локальному идентификатору
	/// </summary>
	public ImmutableDictionary<int, BlockEntity> Blocks { get; private set; } = ImmutableDictionary<int, BlockEntity>.Empty;

	/// <summary>
	/// Источники по идентификатору
	/// </summary>
	public ImmutableDictionary<int, SourceEntity> Sources { get; private set; } = ImmutableDictionary<int, SourceEntity>.Empty;

	/// <summary>
	/// Теги по глобальному идентификатору
	/// </summary>
	public ImmutableDictionary<int, TagEntity> Tags { get; private set; } = ImmutableDictionary<int, TagEntity>.Empty;

	/// <summary>
	/// Учетные записи по идентификатору
	/// </summary>
	public ImmutableDictionary<Guid, UserEntity> Users { get; private set; } = ImmutableDictionary<Guid, UserEntity>.Empty;

	/// <summary>
	/// Группы учетных записей по идентификатору
	/// </summary>
	public ImmutableDictionary<Guid, UserGroupEntity> UserGroups { get; private set; } = ImmutableDictionary<Guid, UserGroupEntity>.Empty;

	#endregion Коллекции с первичными ключами

	#region Коллекции без ключей

	/// <summary>
	/// Правила доступа
	/// </summary>
	public required ImmutableList<AccessRuleEntity> AccessRules { get; init; }

	/// <summary>
	/// Свойства блоков
	/// </summary>
	public required ImmutableList<BlockPropertyEntity> BlockProperties { get; init; }

	/// <summary>
	/// Связи блоков с тегами
	/// </summary>
	public required ImmutableList<BlockTagEntity> BlockTags { get; init; }

	/// <summary>
	/// Связи тегов с входными тегами
	/// </summary>
	public required ImmutableList<TagInputEntity> TagInputs { get; init; }

	/// <summary>
	/// Связи групп с учетными записями
	/// </summary>
	public required ImmutableList<UserGroupRelationEntity> UserGroupRelations { get; init; }

	#endregion Коллекции без ключей

	#region Словари активных объектов (только не удаленные)

	public void SetActiveDictionaries()
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

	/// <summary>
	/// Активные блоки по идентификатору
	/// </summary>
	public ImmutableDictionary<int, BlockEntity> ActiveBlocksById { get; private set; } = ImmutableDictionary<int, BlockEntity>.Empty;

	/// <summary>
	/// Активные источники по идентификатору
	/// </summary>
	public ImmutableDictionary<int, SourceEntity> ActiveSourcesById { get; private set; } = ImmutableDictionary<int, SourceEntity>.Empty;

	/// <summary>
	/// Активные теги по глобальному идентификатору
	/// </summary>
	public ImmutableDictionary<Guid, TagEntity> ActiveTagsByGuid { get; private set; } = ImmutableDictionary<Guid, TagEntity>.Empty;

	/// <summary>
	/// Активные теги по локальному идентификатору
	/// </summary>
	public ImmutableDictionary<int, TagEntity> ActiveTagsById { get; private set; } = ImmutableDictionary<int, TagEntity>.Empty;

	/// <summary>
	/// Активные пользователи по идентификатору
	/// </summary>
	public ImmutableDictionary<Guid, UserEntity> ActiveUsersByGuid { get; private set; } = ImmutableDictionary<Guid, UserEntity>.Empty;

	/// <summary>
	/// Активные группы пользователей по идентификатору
	/// </summary>
	public ImmutableDictionary<Guid, UserGroupEntity> ActiveUserGroupsByGuid { get; private set; } = ImmutableDictionary<Guid, UserGroupEntity>.Empty;

	#endregion Словари активных объектов (только не удаленные)

	#region Коллекции активных объектов (только не удаленные)

	/// <summary>
	/// Активные блоки
	/// </summary>
	public IEnumerable<BlockEntity> ActiveBlocks => ActiveBlocksById.Values;

	/// <summary>
	/// Активные источники
	/// </summary>
	public IEnumerable<SourceEntity> ActiveSources => ActiveSourcesById.Values;

	/// <summary>
	/// Активные теги
	/// </summary>
	public IEnumerable<TagEntity> ActiveTags => ActiveTagsById.Values;

	/// <summary>
	/// Активные группы пользователей
	/// </summary>
	public IEnumerable<UserEntity> ActiveUsers => ActiveUsersByGuid.Values;

	/// <summary>
	/// Активные группы пользователей
	/// </summary>
	public IEnumerable<UserGroupEntity> ActiveUserGroups => ActiveUserGroupsByGuid.Values;

	#endregion Коллекции активных объектов (только не удаленные)

	#region Вспомогательные методы для модификации

	/// <summary>
	/// Обновляет блок в состоянии
	/// </summary>
	public InventoryState WithBlock(BlockEntity block)
	{
		return Update(state => state with
		{
			Blocks = Apply(Blocks, block),
			ActiveBlocksById = ApplyAsActive(ActiveBlocksById, block),
		});
	}

	/// <summary>
	/// добавляет или изменяет источник в состоянии
	/// </summary>
	public InventoryState WithSource(SourceEntity source)
	{
		return Update(state => state with
		{
			Sources = Apply(Sources, source),
			ActiveSourcesById = ApplyAsActive(ActiveSourcesById, source),
		});
	}

	/// <summary>
	/// Обновляет тег в состоянии
	/// </summary>
	public InventoryState WithTag(TagEntity tag)
	{
		return Update(state => state with
		{
			Tags = Apply(Tags, tag),
			ActiveTagsById = ApplyAsActive(ActiveTagsById, tag),
			ActiveTagsByGuid = ApplyAsActive(ActiveTagsByGuid, tag),
		});
	}

	/// <summary>
	/// Обновляет пользователя в состоянии
	/// </summary>
	public InventoryState WithUser(UserEntity user)
	{
		return Update(state => state with
		{
			Users = Apply(Users, user),
			ActiveUsersByGuid = ApplyAsActive(ActiveUsersByGuid, user),
		});
	}

	/// <summary>
	/// Обновляет группу пользователей в состоянии
	/// </summary>
	public InventoryState WithUserGroup(UserGroupEntity userGroup)
	{
		return Update(state => state with
		{
			UserGroups = Apply(UserGroups, userGroup),
			ActiveUserGroupsByGuid = ApplyAsActive(ActiveUserGroupsByGuid, userGroup),
		});
	}

	/// <summary>
	/// Обновляет связи блока с тегами
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="blockTags">Новые связи с тегами</param>
	public InventoryState WithBlockTags(int blockId, IEnumerable<BlockTagEntity> blockTags)
	{
		return this with
		{
			BlockTags = BlockTags.RemoveAll(x => x.Id == blockId).AddRange(blockTags),
		};
	}
	// другие по необходимости с точечными, экономными изменениями

	#endregion

	#region Внутренние методы-хэлперы

	private InventoryState Update(Func<InventoryState, InventoryState> updateFunc)
	{
		InventoryState newState = updateFunc(this);
		newState.UpdateVersion();
		return newState;
	}

	public static ImmutableDictionary<int, TEntity> Apply<TEntity>(ImmutableDictionary<int, TEntity> dict, TEntity entity)
		where TEntity : IWithIdentityKey
	{
		return dict.ContainsKey(entity.Id) ? dict.SetItem(entity.Id, entity) : dict.Add(entity.Id, entity);
	}

	public static ImmutableDictionary<Guid, TEntity> Apply<TEntity>(ImmutableDictionary<Guid, TEntity> dict, TEntity entity)
		where TEntity : IWithGuidKey
	{
		return dict.ContainsKey(entity.Guid) ? dict.SetItem(entity.Guid, entity) : dict.Add(entity.Guid, entity);
	}

	public static ImmutableDictionary<int, TEntity> ApplyAsActive<TEntity>(ImmutableDictionary<int, TEntity> dict, TEntity entity)
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

	public static ImmutableDictionary<Guid, TEntity> ApplyAsActive<TEntity>(ImmutableDictionary<Guid, TEntity> dict, TEntity entity)
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