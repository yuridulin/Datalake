using Datalake.Database.Tables;
using System.Collections.Immutable;

namespace Datalake.Database.InMemory.Models;

/// <summary>
/// Снимок данных БД
/// </summary>
public record class DatalakeDataState
{
	/// <summary>
	/// Версия снимка
	/// </summary>
	public long Version { get; init; } = DateTime.UtcNow.Ticks;

	#region Таблицы

	/// <summary>
	/// Снимок коллекции прав доступа
	/// </summary>
	public required ImmutableList<AccessRights> AccessRights { get; init; }

	/// <summary>
	/// Снимок коллекции блоков
	/// </summary>
	public required ImmutableList<Block> Blocks { get; init; }

	/// <summary>
	/// Снимок коллекции свойств блоков
	/// </summary>
	public required ImmutableList<BlockProperty> BlockProperties { get; init; }

	/// <summary>
	/// Снимок коллекции связей блоков с тегами
	/// </summary>
	public required ImmutableList<BlockTag> BlockTags { get; init; }

	/// <summary>
	/// Снимок коллекции источников
	/// </summary>
	public required ImmutableList<Source> Sources { get; init; }

	/// <summary>
	/// Снимок коллекции настроек
	/// </summary>
	public required Settings Settings { get; init; }

	/// <summary>
	/// Снимок коллекции тегов
	/// </summary>
	public required ImmutableList<Tag> Tags { get; init; }

	/// <summary>
	/// Снимок коллекции входных тегов формул
	/// </summary>
	public required ImmutableList<TagInput> TagInputs { get; init; }

	/// <summary>
	/// Снимок коллекции учетных записей
	/// </summary>
	public required ImmutableList<User> Users { get; init; }

	/// <summary>
	/// Снимок коллекции групп учетных записей
	/// </summary>
	public required ImmutableList<UserGroup> UserGroups { get; init; }

	/// <summary>
	/// Снимок коллекции связей групп с учетными записями
	/// </summary>
	public required ImmutableList<UserGroupRelation> UserGroupRelations { get; init; }

	/// <summary>
	/// Снимок коллекции сессий доступа
	/// </summary>
	public required ImmutableList<UserSession> UserSessions { get; init; }

	#endregion Таблицы

	#region Словари

	internal void InitDictionaries()
	{
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
	public ImmutableDictionary<int, Block> BlocksById { get; private set; } = ImmutableDictionary<int, Block>.Empty;

	/// <summary>
	/// Источники по идентификатору, без удаленных
	/// </summary>
	public ImmutableDictionary<int, Source> SourcesById { get; private set; } = ImmutableDictionary<int, Source>.Empty;

	/// <summary>
	/// Теги по глобальному идентификатору, без удаленных
	/// </summary>
	public ImmutableDictionary<Guid, Tag> TagsByGuid { get; private set; } = ImmutableDictionary<Guid, Tag>.Empty;

	/// <summary>
	/// Теги по локальному идентификатору, без удаленных
	/// </summary>
	public ImmutableDictionary<int, Tag> TagsById { get; private set; } = ImmutableDictionary<int, Tag>.Empty;

	/// <summary>
	/// Учетные записи по идентификатору, без удаленных
	/// </summary>
	public ImmutableDictionary<Guid, User> UsersByGuid { get; private set; } = ImmutableDictionary<Guid, User>.Empty;

	/// <summary>
	/// Группы учетных записей по идентификатору, без удаленных
	/// </summary>
	public ImmutableDictionary<Guid, UserGroup> UserGroupsByGuid { get; private set; } = ImmutableDictionary<Guid, UserGroup>.Empty;

	#endregion Словари
}