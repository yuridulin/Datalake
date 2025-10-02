using Datalake.Inventory.Domain.Entities;
using System.Collections.Immutable;

namespace Datalake.Inventory.Application.Interfaces.InMemory;

/// <summary>
/// Снимок данных БД
/// </summary>
public interface IInventoryCacheState
{
	/// <summary>
	/// Версия снимка
	/// </summary>
	long Version { get; }

	#region Коллекции с первичными ключами

	/// <summary>
	/// Блоки по локальному идентификатору
	/// </summary>
	ImmutableDictionary<int, BlockEntity> Blocks { get; }

	/// <summary>
	/// Источники по идентификатору
	/// </summary>
	ImmutableDictionary<int, SourceEntity> Sources { get; }

	/// <summary>
	/// Теги по глобальному идентификатору
	/// </summary>
	ImmutableDictionary<int, TagEntity> Tags { get; }

	/// <summary>
	/// Учетные записи по идентификатору
	/// </summary>
	ImmutableDictionary<Guid, UserEntity> Users { get; }

	/// <summary>
	/// Группы учетных записей по идентификатору
	/// </summary>
	ImmutableDictionary<Guid, UserGroupEntity> UserGroups { get; }

	#endregion Коллекции с первичными ключами

	#region Коллекции без ключей

	/// <summary>
	/// Правила доступа
	/// </summary>
	ImmutableList<AccessRuleEntity> AccessRules { get; init; }

	/// <summary>
	/// Свойства блоков
	/// </summary>
	ImmutableList<BlockPropertyEntity> BlockProperties { get; init; }

	/// <summary>
	/// Связи блоков с тегами
	/// </summary>
	ImmutableList<BlockTagEntity> BlockTags { get; init; }

	/// <summary>
	/// Связи тегов с входными тегами
	/// </summary>
	ImmutableList<TagInputEntity> TagInputs { get; init; }

	/// <summary>
	/// Связи тегов с пороговыми уставками
	/// </summary>
	ImmutableList<TagThresholdEntity> TagThresholds { get; init; }

	/// <summary>
	/// Связи групп с учетными записями
	/// </summary>
	ImmutableList<UserGroupRelationEntity> UserGroupRelations { get; init; }

	#endregion Коллекции без ключей

	#region Словари активных объектов (только не удаленные)

	/// <summary>
	/// Активные блоки по идентификатору
	/// </summary>
	ImmutableDictionary<int, BlockEntity> ActiveBlocksById { get; }

	/// <summary>
	/// Активные источники по идентификатору
	/// </summary>
	ImmutableDictionary<int, SourceEntity> ActiveSourcesById { get; }

	/// <summary>
	/// Активные теги по глобальному идентификатору
	/// </summary>
	ImmutableDictionary<Guid, TagEntity> ActiveTagsByGuid { get; }

	/// <summary>
	/// Активные теги по локальному идентификатору
	/// </summary>
	ImmutableDictionary<int, TagEntity> ActiveTagsById { get; }

	/// <summary>
	/// Активные пользователи по идентификатору
	/// </summary>
	ImmutableDictionary<Guid, UserEntity> ActiveUsersByGuid { get; }

	/// <summary>
	/// Активные группы пользователей по идентификатору
	/// </summary>
	ImmutableDictionary<Guid, UserGroupEntity> ActiveUserGroupsByGuid { get; }

	#endregion Словари активных объектов (только не удаленные)

	#region Коллекции активных объектов (только не удаленные)

	/// <summary>
	/// Активные блоки
	/// </summary>
	IEnumerable<BlockEntity> ActiveBlocks { get; }

	/// <summary>
	/// Активные источники
	/// </summary>
	IEnumerable<SourceEntity> ActiveSources { get; }

	/// <summary>
	/// Активные теги
	/// </summary>
	IEnumerable<TagEntity> ActiveTags { get; }

	/// <summary>
	/// Активные группы пользователей
	/// </summary>
	IEnumerable<UserEntity> ActiveUsers { get; }

	/// <summary>
	/// Активные группы пользователей
	/// </summary>
	IEnumerable<UserGroupEntity> ActiveUserGroups { get; }

	#endregion Коллекции активных объектов (только не удаленные)

	#region Вспомогательные методы для модификации

	/// <summary>
	/// Обновляет блок в состоянии
	/// </summary>
	IInventoryCacheState WithBlock(BlockEntity block);

	/// <summary>
	/// добавляет или изменяет источник в состоянии
	/// </summary>
	IInventoryCacheState WithSource(SourceEntity source);

	/// <summary>
	/// Обновляет тег в состоянии
	/// </summary>
	IInventoryCacheState WithTag(TagEntity tag);

	/// <summary>
	/// Обновляет пользователя в состоянии
	/// </summary>
	IInventoryCacheState WithUser(UserEntity user);

	/// <summary>
	/// Обновляет группу пользователей в состоянии
	/// </summary>
	IInventoryCacheState WithUserGroup(UserGroupEntity userGroup);

	/// <summary>
	/// Обновляет связи блока с тегами
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="blockTags">Новые связи с тегами</param>
	IInventoryCacheState WithBlockTags(int blockId, IEnumerable<BlockTagEntity> blockTags);

	/// <summary>
	/// Обновляет связи тега с блоками
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="blockTags">Новые связи с блоками</param>
	IInventoryCacheState WithTagBlocks(int tagId, IEnumerable<BlockTagEntity> blockTags);

	/// <summary>
	/// Обновляет связи тега с блоками
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="tagInputs">Новые связи с входными тегами</param>
	IInventoryCacheState WithTagInputs(int tagId, IEnumerable<TagInputEntity> tagInputs);

	/// <summary>
	/// Обновляет связи тега с блоками
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="tagThresholds">Новые связи с пороговыми уставками</param>
	IInventoryCacheState WithTagThresholds(int tagId, IEnumerable<TagThresholdEntity> tagThresholds);

	/// <summary>
	/// Обновляет связи группы учетных записей с учетными записями
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы учетных записей</param>
	/// <param name="userGroupRelations">Новые связи с учетными записями</param>
	IInventoryCacheState WithUserGroupRelations(Guid userGroupGuid, IEnumerable<UserGroupRelationEntity> userGroupRelations);

	/// <summary>
	/// Обновляет правила доступа, удаляя старые и добавляя новые
	/// </summary>
	/// <param name="oldRulesId">Идентификаторы старых правил</param>
	/// <param name="newRules">Новые правила</param>

	IInventoryCacheState WithAccessRules(int[] oldRulesId, AccessRuleEntity[] newRules);

	#endregion
}