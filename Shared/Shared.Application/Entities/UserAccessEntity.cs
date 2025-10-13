using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Exceptions;

namespace Datalake.Shared.Application.Entities;

public record struct UserAccessEntity
{
	public UserAccessEntity(
		Guid guid,
		Guid? energoId,
		AccessRuleValue rootRule,
		Dictionary<Guid, AccessRuleValue>? groupsRules = null,
		Dictionary<int, AccessRuleValue>? sourcesRules = null,
		Dictionary<int, AccessRuleValue>? blocksRules = null,
		Dictionary<int, AccessRuleValue>? tagsRules = null)
	{
		Guid = guid;
		EnergoId = energoId;
		RootRule = rootRule;
		GroupsRules = groupsRules ?? [];
		SourcesRules = sourcesRules ?? [];
		BlocksRules = blocksRules ?? [];
		TagsRules = tagsRules ?? [];
	}

	public Guid Guid { get; private set; }

	public Guid? EnergoId { get; private set; }

	public AccessRuleValue RootRule { get; private set; }

	public Dictionary<Guid, AccessRuleValue> GroupsRules { get; private set; }

	public Dictionary<int, AccessRuleValue> SourcesRules { get; private set; }

	public Dictionary<int, AccessRuleValue> BlocksRules { get; private set; }

	public Dictionary<int, AccessRuleValue> TagsRules { get; private set; }

	public void AddGroupRule(Guid groupGuid, AccessRuleValue rule)
	{
		GroupsRules[groupGuid] = rule;
	}

	public void AddSourceRule(int sourceId, AccessRuleValue rule)
	{
		SourcesRules[sourceId] = rule;
	}

	public void AddBlockRule(int blockId, AccessRuleValue rule)
	{
		BlocksRules[blockId] = rule;
	}

	public void AddTagRule(int tagId, AccessRuleValue rule)
	{
		TagsRules[tagId] = rule;
	}

	#region Проверки

	/// <summary>
	/// Проверка достаточности глобального уровня доступа
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
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
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
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
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
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
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
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
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
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
	public AccessRuleValue GetAccessToSource(int sourceId)
	{
		return SourcesRules.TryGetValue(sourceId, out var rule) ? rule : RootRule;
	}

	/// <summary>
	/// Получение правила доступа к блоку
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <returns>Правило доступа</returns>
	public AccessRuleValue GetAccessToBlock(int blockId)
	{
		return BlocksRules.TryGetValue(blockId, out var rule) ? rule : RootRule;
	}

	/// <summary>
	/// Получение правила доступа к тегу
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <returns>Правило доступа</returns>
	public AccessRuleValue GetAccessToTag(int tagId)
	{
		return TagsRules.TryGetValue(tagId, out var rule) ? rule : RootRule;
	}

	/// <summary>
	/// Получение правила доступа к группе пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Правило доступа</returns>
	public AccessRuleValue GetAccessToUserGroup(Guid groupGuid)
	{
		return GroupsRules.TryGetValue(groupGuid, out var rule) ? rule : RootRule;
	}

	#endregion Получение
}
