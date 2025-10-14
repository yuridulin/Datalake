using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Exceptions;

namespace Datalake.Shared.Application.Entities;

public record struct UserAccessValue
{
	public UserAccessValue(
		Guid guid,
		Guid? energoId,
		UserAccessRuleValue rootRule,
		Dictionary<Guid, UserAccessRuleValue>? groupsRules = null,
		Dictionary<int, UserAccessRuleValue>? sourcesRules = null,
		Dictionary<int, UserAccessRuleValue>? blocksRules = null,
		Dictionary<int, UserAccessRuleValue>? tagsRules = null)
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

	public UserAccessRuleValue RootRule { get; private set; }

	public Dictionary<Guid, UserAccessRuleValue> GroupsRules { get; private set; }

	public Dictionary<int, UserAccessRuleValue> SourcesRules { get; private set; }

	public Dictionary<int, UserAccessRuleValue> BlocksRules { get; private set; }

	public Dictionary<int, UserAccessRuleValue> TagsRules { get; private set; }

	public readonly void AddGroupRule(Guid groupGuid, UserAccessRuleValue rule)
	{
		GroupsRules[groupGuid] = rule;
	}

	public readonly void AddSourceRule(int sourceId, UserAccessRuleValue rule)
	{
		SourcesRules[sourceId] = rule;
	}

	public readonly void AddBlockRule(int blockId, UserAccessRuleValue rule)
	{
		BlocksRules[blockId] = rule;
	}

	public readonly void AddTagRule(int tagId, UserAccessRuleValue rule)
	{
		TagsRules[tagId] = rule;
	}

	#region Проверки

	/// <summary>
	/// Проверка достаточности глобального уровня доступа
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
	public readonly bool HasGlobalAccess(AccessType minimalAccess)
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
	public readonly bool HasAccessToSource(AccessType minimalAccess, int sourceId)
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
	public readonly bool HasAccessToBlock(AccessType minimalAccess, int blockId)
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
	public readonly bool HasAccessToTag(AccessType minimalAccess, int tagId)
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
	public readonly bool HasAccessToUserGroup(AccessType minimalAccess, Guid groupGuid)
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
	public readonly void ThrowIfNoGlobalAccess(AccessType minimalAccess)
	{
		if (!HasGlobalAccess(minimalAccess))
			throw NoAccessUser(Guid);
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к источнику данных
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="sourceId">Идентификатор источника</param>
	public readonly void ThrowIfNoAccessToSource(AccessType minimalAccess, int sourceId)
	{
		if (!HasAccessToSource(minimalAccess, sourceId))
			throw NoAccessUser(Guid);
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к блоку
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="blockId">Идентификатор блока</param>
	public readonly void ThrowIfNoAccessToBlock(AccessType minimalAccess, int blockId)
	{
		if (!HasAccessToBlock(minimalAccess, blockId))
			throw NoAccessUser(Guid);
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к тегу
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="tagId">Идентификатор тега</param>
	public readonly void ThrowIfNoAccessToTag(AccessType minimalAccess, int tagId)
	{
		if (!HasAccessToTag(minimalAccess, tagId))
			throw NoAccessUser(Guid);
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к группе пользователей
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	public readonly void ThrowIfNoAccessToUserGroup(AccessType minimalAccess, Guid groupGuid)
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
	public readonly UserAccessRuleValue GetAccessToSource(int sourceId)
	{
		return SourcesRules.TryGetValue(sourceId, out var rule) ? rule : RootRule;
	}

	/// <summary>
	/// Получение правила доступа к блоку
	/// </summary>
	/// <param name="blockId">Идентификатор блока</param>
	/// <returns>Правило доступа</returns>
	public readonly UserAccessRuleValue GetAccessToBlock(int blockId)
	{
		return BlocksRules.TryGetValue(blockId, out var rule) ? rule : RootRule;
	}

	/// <summary>
	/// Получение правила доступа к тегу
	/// </summary>
	/// <param name="tagId">Идентификатор тега</param>
	/// <returns>Правило доступа</returns>
	public readonly UserAccessRuleValue GetAccessToTag(int tagId)
	{
		return TagsRules.TryGetValue(tagId, out var rule) ? rule : RootRule;
	}

	/// <summary>
	/// Получение правила доступа к группе пользователей
	/// </summary>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Правило доступа</returns>
	public readonly UserAccessRuleValue GetAccessToUserGroup(Guid groupGuid)
	{
		return GroupsRules.TryGetValue(groupGuid, out var rule) ? rule : RootRule;
	}

	#endregion Получение
}
