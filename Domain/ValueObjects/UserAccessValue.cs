using Datalake.Domain.Enums;
using Datalake.Domain.Exceptions;

namespace Datalake.Domain.ValueObjects;

/// <summary>
/// Состояние рассчитанных прав доступа учетной записи
/// </summary>
public record class UserAccessValue
{
	/// <summary>
	/// Конструктор
	/// </summary>
	/// <param name="userGuid">Идентификатор учетной записи</param>
	/// <param name="rootRule">Глобальный уровень доступа</param>
	/// <param name="groupsRules">Уровни доступа к группам учетных групп</param>
	/// <param name="sourcesRules">Уровни доступа к источникам данных</param>
	/// <param name="blocksRules">Уровни доступа к блокам</param>
	/// <param name="tagsRules">Уровни доступа к тегам</param>
	public UserAccessValue(
		Guid userGuid,
		UserAccessRuleValue rootRule,
		Dictionary<Guid, UserAccessRuleValue>? groupsRules = null,
		Dictionary<int, UserAccessRuleValue>? sourcesRules = null,
		Dictionary<int, UserAccessRuleValue>? blocksRules = null,
		Dictionary<int, UserAccessRuleValue>? tagsRules = null)
	{
		Guid = userGuid;
		RootRule = rootRule;
		GroupsRules = groupsRules ?? [];
		SourcesRules = sourcesRules ?? [];
		BlocksRules = blocksRules ?? [];
		TagsRules = tagsRules ?? [];
	}

	#region Свойства

	/// <summary>
	/// Идентификатор учетной записи
	/// </summary>
	public Guid Guid { get; private set; }

	/// <summary>
	/// Глобальный уровень доступа
	/// </summary>
	public UserAccessRuleValue RootRule { get; private set; }

	/// <summary>
	/// Уровни доступа к группам учетных групп
	/// </summary>
	public Dictionary<Guid, UserAccessRuleValue> GroupsRules { get; private set; }

	/// <summary>
	/// Уровни доступа к источникам данных
	/// </summary>
	public Dictionary<int, UserAccessRuleValue> SourcesRules { get; private set; }

	/// <summary>
	/// Уровни доступа к блокам
	/// </summary>
	public Dictionary<int, UserAccessRuleValue> BlocksRules { get; private set; }

	/// <summary>
	/// Уровни доступа к тегам
	/// </summary>
	public Dictionary<int, UserAccessRuleValue> TagsRules { get; private set; }

	#endregion

	#region Методы

	/// <summary>
	/// Добавление доступа к группе учетных записей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы учетной записи</param>
	/// <param name="rule">Правило доступа</param>
	public void AddGroupRule(Guid groupGuid, UserAccessRuleValue rule)
	{
		GroupsRules[groupGuid] = rule;
	}

	/// <summary>
	/// Добавление доступа к источнику доступа
	/// </summary>
	/// <param name="sourceId">Идентификатор источника данных</param>
	/// <param name="rule">Правило доступа</param>
	public void AddSourceRule(int sourceId, UserAccessRuleValue rule)
	{
		SourcesRules[sourceId] = rule;
	}

	/// <summary>
	/// Добавление доступа к блоку
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="rule">Правило доступа</param>
	public void AddBlockRule(int blockId, UserAccessRuleValue rule)
	{
		BlocksRules[blockId] = rule;
	}

	/// <summary>
	/// Добавление доступа к тегу
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="rule">Правило доступа</param>
	public void AddTagRule(int tagId, UserAccessRuleValue rule)
	{
		TagsRules[tagId] = rule;
	}

	#endregion Методы

	#region Проверки

	/// <summary>
	/// Проверка достаточности глобального уровня доступа
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	public bool HasGlobalAccess(AccessType minimalAccess)
	{
		bool hasAccess = RootRule.HasAccess(minimalAccess);
		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к источнику данных
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="sourceId">Идентификатор источника</param>
	public bool HasAccessToSource(AccessType minimalAccess, int sourceId)
	{
		var access = SourcesRules.TryGetValue(sourceId, out var rule) ? rule : RootRule;
		var hasAccess = access.HasAccess(minimalAccess);
		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к блоку
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="blockId">Идентификатор блока</param>
	public bool HasAccessToBlock(AccessType minimalAccess, int blockId)
	{
		var access = BlocksRules.TryGetValue(blockId, out var rule) ? rule : RootRule;
		var hasAccess = access.HasAccess(minimalAccess);
		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к тегу
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="tagId">Идентификатор тега</param>
	public bool HasAccessToTag(AccessType minimalAccess, int tagId)
	{
		var access = TagsRules.TryGetValue(tagId, out var rule) ? rule : RootRule;
		var hasAccess = access.HasAccess(minimalAccess);
		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к группе пользователей
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	public bool HasAccessToUserGroup(AccessType minimalAccess, Guid groupGuid)
	{
		var access = GroupsRules.TryGetValue(groupGuid, out var rule) ? rule : RootRule;
		var hasAccess = access.HasAccess(minimalAccess);
		return hasAccess;
	}

	#endregion Проверки

	#region Ошибки

	/// <summary>
	/// Нет доступа для локального пользователя
	/// </summary>
	public static UnauthorizedException NoAccessUser(Guid userGuid)
		=> new("нет доступа.\nПользователь: [" + userGuid.ToString() + "]");

	/// <summary>
	/// Проверка достаточности глобального уровня доступа
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	public void ThrowIfNoGlobalAccess(AccessType minimalAccess)
	{
		if (!HasGlobalAccess(minimalAccess))
			throw NoAccessUser(Guid);
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к источнику данных
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="sourceId">Идентификатор источника</param>
	public void ThrowIfNoAccessToSource(AccessType minimalAccess, int sourceId)
	{
		if (!HasAccessToSource(minimalAccess, sourceId))
			throw NoAccessUser(Guid);
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к блоку
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="blockId">Идентификатор блока</param>
	public void ThrowIfNoAccessToBlock(AccessType minimalAccess, int blockId)
	{
		if (!HasAccessToBlock(minimalAccess, blockId))
			throw NoAccessUser(Guid);
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к тегу
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="tagId">Идентификатор тега</param>
	public void ThrowIfNoAccessToTag(AccessType minimalAccess, int tagId)
	{
		if (!HasAccessToTag(minimalAccess, tagId))
			throw NoAccessUser(Guid);
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к группе пользователей
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	public void ThrowIfNoAccessToUserGroup(AccessType minimalAccess, Guid groupGuid)
	{
		if (!HasAccessToUserGroup(minimalAccess, groupGuid))
			throw NoAccessUser(Guid);
	}

	#endregion Ошибки

	#region Получение

	/// <summary>
	/// Получение правила доступа к источнику данных
	/// </summary>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <returns>Правило доступа</returns>
	public UserAccessRuleValue GetAccessToSource(int sourceId)
	{
		return SourcesRules.TryGetValue(sourceId, out var rule) ? rule : RootRule;
	}

	/// <summary>
	/// Получение правила доступа к блоку
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <returns>Правило доступа</returns>
	public UserAccessRuleValue GetAccessToBlock(int blockId)
	{
		return BlocksRules.TryGetValue(blockId, out var rule) ? rule : RootRule;
	}

	/// <summary>
	/// Получение правила доступа к тегу
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <returns>Правило доступа</returns>
	public UserAccessRuleValue GetAccessToTag(int tagId)
	{
		return TagsRules.TryGetValue(tagId, out var rule) ? rule : RootRule;
	}

	/// <summary>
	/// Получение правила доступа к группе пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Правило доступа</returns>
	public UserAccessRuleValue GetAccessToUserGroup(Guid groupGuid)
	{
		return GroupsRules.TryGetValue(groupGuid, out var rule) ? rule : RootRule;
	}

	#endregion Получение
}
