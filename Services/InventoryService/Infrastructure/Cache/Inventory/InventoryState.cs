using Datalake.InventoryService.Domain.Entities;
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

	#region Таблицы

	/// <summary>
	/// Снимок коллекции прав доступа
	/// </summary>
	public required ImmutableList<AccessRuleEntity> AccessRights { get; init; }

	/// <summary>
	/// Снимок коллекции блоков
	/// </summary>
	public required ImmutableList<BlockEntity> Blocks { get; init; }

	/// <summary>
	/// Снимок коллекции свойств блоков
	/// </summary>
	public required ImmutableList<BlockPropertyEntity> BlockProperties { get; init; }

	/// <summary>
	/// Снимок коллекции связей блоков с тегами
	/// </summary>
	public required ImmutableList<BlockTagEntity> BlockTags { get; init; }

	/// <summary>
	/// Снимок коллекции источников
	/// </summary>
	public required ImmutableList<SourceEntity> Sources { get; init; }

	/// <summary>
	/// Снимок коллекции тегов
	/// </summary>
	public required ImmutableList<TagEntity> Tags { get; init; }

	/// <summary>
	/// Снимок коллекции входных тегов формул
	/// </summary>
	public required ImmutableList<TagInputEntity> TagInputs { get; init; }

	/// <summary>
	/// Снимок коллекции учетных записей
	/// </summary>
	public required ImmutableList<UserEntity> Users { get; init; }

	/// <summary>
	/// Снимок коллекции групп учетных записей
	/// </summary>
	public required ImmutableList<UserGroupEntity> UserGroups { get; init; }

	/// <summary>
	/// Снимок коллекции связей групп с учетными записями
	/// </summary>
	public required ImmutableList<UserGroupRelationEntity> UserGroupRelations { get; init; }

	#endregion Таблицы

	#region Словари

	internal void InitDictionaries()
	{
		Version = DateTime.UtcNow.Ticks;
		BlocksById = Blocks.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Id);
		SourcesById = Sources.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Id);
		TagsByGuid = Tags.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.GlobalGuid);
		TagsById = Tags.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Id);
		UsersByGuid = Users.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Guid);
		UserGroupsByGuid = UserGroups.Where(x => !x.IsDeleted).ToImmutableDictionary(x => x.Guid);
	}

	/// <summary>
	/// Блоки по локальному идентификатору, без удаленных
	/// </summary>
	public ImmutableDictionary<int, BlockEntity> BlocksById { get; private set; } = ImmutableDictionary<int, BlockEntity>.Empty;

	/// <summary>
	/// Источники по идентификатору, без удаленных
	/// </summary>
	public ImmutableDictionary<int, SourceEntity> SourcesById { get; private set; } = ImmutableDictionary<int, SourceEntity>.Empty;

	/// <summary>
	/// Теги по глобальному идентификатору, без удаленных
	/// </summary>
	public ImmutableDictionary<Guid, TagEntity> TagsByGuid { get; private set; } = ImmutableDictionary<Guid, TagEntity>.Empty;

	/// <summary>
	/// Теги по локальному идентификатору, без удаленных
	/// </summary>
	public ImmutableDictionary<int, TagEntity> TagsById { get; private set; } = ImmutableDictionary<int, TagEntity>.Empty;

	/// <summary>
	/// Учетные записи по идентификатору, без удаленных
	/// </summary>
	public ImmutableDictionary<Guid, UserEntity> UsersByGuid { get; private set; } = ImmutableDictionary<Guid, UserEntity>.Empty;

	/// <summary>
	/// Группы учетных записей по идентификатору, без удаленных
	/// </summary>
	public ImmutableDictionary<Guid, UserGroupEntity> UserGroupsByGuid { get; private set; } = ImmutableDictionary<Guid, UserGroupEntity>.Empty;

	#endregion Словари
}