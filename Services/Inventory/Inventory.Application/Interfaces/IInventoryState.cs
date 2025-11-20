using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Models;
using System.Collections.Immutable;

namespace Datalake.Inventory.Application.Interfaces;

/// <summary>
/// Снимок данных БД
/// </summary>
public interface IInventoryState
{
	/// <summary>
	/// Версия снимка
	/// </summary>
	long Version { get; }

	#region Коллекции

	/// <summary>
	/// Блоки по локальному идентификатору
	/// </summary>
	ImmutableDictionary<int, BlockMemoryDto> Blocks { get; }

	/// <summary>
	/// Источники по идентификатору
	/// </summary>
	ImmutableDictionary<int, SourceMemoryDto> Sources { get; }

	/// <summary>
	/// Теги по глобальному идентификатору
	/// </summary>
	ImmutableDictionary<int, TagMemoryDto> Tags { get; }

	/// <summary>
	/// Учетные записи по идентификатору
	/// </summary>
	ImmutableDictionary<Guid, UserMemoryDto> Users { get; }

	/// <summary>
	/// Группы учетных записей по идентификатору
	/// </summary>
	ImmutableDictionary<Guid, UserGroupMemoryDto> UserGroups { get; }

	/// <summary>
	/// Правила доступа
	/// </summary>
	ImmutableList<AccessRightsMemoryDto> AccessRules { get; init; }

	/// <summary>
	/// Связи блоков с тегами
	/// </summary>
	ImmutableList<BlockTagMemoryDto> BlockTags { get; init; }

	/// <summary>
	/// Связи групп с учетными записями
	/// </summary>
	ImmutableList<UserGroupRelationMemoryDto> UserGroupRelations { get; init; }

	#endregion Коллекции

	#region Вспомогательные методы для модификации

	/// <summary>
	/// Обновляет блок в состоянии
	/// </summary>
	IInventoryState WithBlock(Block block);

	/// <summary>
	/// добавляет или изменяет источник в состоянии
	/// </summary>
	IInventoryState WithSource(Source source);

	/// <summary>
	/// Обновляет тег в состоянии
	/// </summary>
	IInventoryState WithTag(Tag tag);

	/// <summary>
	/// Обновляет пользователя в состоянии
	/// </summary>
	IInventoryState WithUser(User user);

	/// <summary>
	/// Обновляет группу пользователей в состоянии
	/// </summary>
	IInventoryState WithUserGroup(UserGroup userGroup);

	/// <summary>
	/// Обновляет связи блока с тегами
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="blockTags">Новые связи с тегами</param>
	IInventoryState WithBlockTags(int blockId, IEnumerable<BlockTag> blockTags);

	/// <summary>
	/// Обновляет связи тега с блоками
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="blockTags">Новые связи с блоками</param>
	IInventoryState WithTagBlocks(int tagId, IEnumerable<BlockTag> blockTags);

	/// <summary>
	/// Обновляет связи группы учетных записей с учетными записями
	/// </summary>
	/// <param name="userGroupGuid">Идентификатор группы учетных записей</param>
	/// <param name="userGroupRelations">Новые связи с учетными записями</param>
	IInventoryState WithUserGroupRelations(Guid userGroupGuid, IEnumerable<UserGroupRelation> userGroupRelations);

	/// <summary>
	/// Обновляет правила доступа, удаляя старые и добавляя новые
	/// </summary>
	/// <param name="oldRulesId">Идентификаторы старых правил</param>
	/// <param name="newRules">Новые правила</param>

	IInventoryState WithAccessRules(int[] oldRulesId, AccessRule[] newRules);

	#endregion
}