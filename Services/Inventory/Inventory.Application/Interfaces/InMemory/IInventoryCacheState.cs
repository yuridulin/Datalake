using Datalake.Domain.Entities;
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
	ImmutableDictionary<int, Block> Blocks { get; }

	/// <summary>
	/// Источники по идентификатору
	/// </summary>
	ImmutableDictionary<int, Source> Sources { get; }

	/// <summary>
	/// Теги по глобальному идентификатору
	/// </summary>
	ImmutableDictionary<int, Tag> Tags { get; }

	/// <summary>
	/// Учетные записи по идентификатору
	/// </summary>
	ImmutableDictionary<Guid, User> Users { get; }

	/// <summary>
	/// Группы учетных записей по идентификатору
	/// </summary>
	ImmutableDictionary<Guid, UserGroup> UserGroups { get; }

	#endregion Коллекции с первичными ключами

	#region Коллекции без ключей

	/// <summary>
	/// Правила доступа
	/// </summary>
	ImmutableList<AccessRights> AccessRules { get; init; }

	/// <summary>
	/// Свойства блоков
	/// </summary>
	ImmutableList<BlockProperty> BlockProperties { get; init; }

	/// <summary>
	/// Связи блоков с тегами
	/// </summary>
	ImmutableList<BlockTag> BlockTags { get; init; }

	/// <summary>
	/// Связи тегов с входными тегами
	/// </summary>
	ImmutableList<TagInput> TagInputs { get; init; }

	/// <summary>
	/// Связи тегов с пороговыми уставками
	/// </summary>
	ImmutableList<TagThreshold> TagThresholds { get; init; }

	/// <summary>
	/// Связи групп с учетными записями
	/// </summary>
	ImmutableList<UserGroupRelation> UserGroupRelations { get; init; }

	#endregion Коллекции без ключей

	#region Словари активных объектов (только не удаленные)

	/// <summary>
	/// Активные блоки по идентификатору
	/// </summary>
	ImmutableDictionary<int, Block> ActiveBlocksById { get; }

	/// <summary>
	/// Активные источники по идентификатору
	/// </summary>
	ImmutableDictionary<int, Source> ActiveSourcesById { get; }

	/// <summary>
	/// Активные теги по глобальному идентификатору
	/// </summary>
	ImmutableDictionary<Guid, Tag> ActiveTagsByGuid { get; }

	/// <summary>
	/// Активные теги по локальному идентификатору
	/// </summary>
	ImmutableDictionary<int, Tag> ActiveTagsById { get; }

	/// <summary>
	/// Активные пользователи по идентификатору
	/// </summary>
	ImmutableDictionary<Guid, User> ActiveUsersByGuid { get; }

	/// <summary>
	/// Активные группы пользователей по идентификатору
	/// </summary>
	ImmutableDictionary<Guid, UserGroup> ActiveUserGroupsByGuid { get; }

	#endregion Словари активных объектов (только не удаленные)

	#region Коллекции активных объектов (только не удаленные)

	/// <summary>
	/// Активные блоки
	/// </summary>
	IEnumerable<Block> ActiveBlocks { get; }

	/// <summary>
	/// Активные источники
	/// </summary>
	IEnumerable<Source> ActiveSources { get; }

	/// <summary>
	/// Активные теги
	/// </summary>
	IEnumerable<Tag> ActiveTags { get; }

	/// <summary>
	/// Активные группы пользователей
	/// </summary>
	IEnumerable<User> ActiveUsers { get; }

	/// <summary>
	/// Активные группы пользователей
	/// </summary>
	IEnumerable<UserGroup> ActiveUserGroups { get; }

	#endregion Коллекции активных объектов (только не удаленные)

	#region Вспомогательные методы для модификации

	/// <summary>
	/// Обновляет блок в состоянии
	/// </summary>
	IInventoryCacheState WithBlock(Block block);

	/// <summary>
	/// добавляет или изменяет источник в состоянии
	/// </summary>
	IInventoryCacheState WithSource(Source source);

	/// <summary>
	/// Обновляет тег в состоянии
	/// </summary>
	IInventoryCacheState WithTag(Tag tag);

	/// <summary>
	/// Обновляет пользователя в состоянии
	/// </summary>
	IInventoryCacheState WithUser(User user);

	/// <summary>
	/// Обновляет группу пользователей в состоянии
	/// </summary>
	IInventoryCacheState WithUserGroup(UserGroup userGroup);

	/// <summary>
	/// Обновляет связи блока с тегами
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="blockTags">Новые связи с тегами</param>
	IInventoryCacheState WithBlockTags(int blockId, IEnumerable<BlockTag> blockTags);

	/// <summary>
	/// Обновляет связи тега с блоками
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="blockTags">Новые связи с блоками</param>
	IInventoryCacheState WithTagBlocks(int tagId, IEnumerable<BlockTag> blockTags);

	/// <summary>
	/// Обновляет связи тега с блоками
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="tagInputs">Новые связи с входными тегами</param>
	IInventoryCacheState WithTagInputs(int tagId, IEnumerable<TagInput> tagInputs);

	/// <summary>
	/// Обновляет связи тега с блоками
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="tagThresholds">Новые связи с пороговыми уставками</param>
	IInventoryCacheState WithTagThresholds(int tagId, IEnumerable<TagThreshold> tagThresholds);

	/// <summary>
	/// Обновляет связи группы учетных записей с учетными записями
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы учетных записей</param>
	/// <param name="userGroupRelations">Новые связи с учетными записями</param>
	IInventoryCacheState WithUserGroupRelations(Guid userGroupGuid, IEnumerable<UserGroupRelation> userGroupRelations);

	/// <summary>
	/// Обновляет правила доступа, удаляя старые и добавляя новые
	/// </summary>
	/// <param name="oldRulesId">Идентификаторы старых правил</param>
	/// <param name="newRules">Новые правила</param>

	IInventoryCacheState WithAccessRules(int[] oldRulesId, AccessRights[] newRules);

	#endregion
}