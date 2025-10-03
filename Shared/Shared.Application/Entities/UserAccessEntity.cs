using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Exceptions;

namespace Datalake.Shared.Application.Entities;

public class UserAccessEntity(
	Guid guid,
	Guid? energoId,
	AccessRuleValue rootRule,
	Dictionary<Guid, AccessRuleValue>? groupsRules = null,
	Dictionary<int, AccessRuleValue>? sourcesRules = null,
	Dictionary<int, AccessRuleValue>? blocksRules = null,
	Dictionary<int, AccessRuleValue>? tagsRules = null,
	UserAccessEntity? underlyingUser = null)
{
	public Guid Guid { get; private set; } = guid;

	public Guid? EnergoId { get; private set; } = energoId;

	public AccessRuleValue RootRule { get; private set; } = rootRule;

	public Dictionary<Guid, AccessRuleValue> GroupsRules { get; private set; } = groupsRules ?? [];

	public Dictionary<int, AccessRuleValue> SourcesRules { get; private set; } = sourcesRules ?? [];

	public Dictionary<int, AccessRuleValue> BlocksRules { get; private set; } = blocksRules ?? [];

	public Dictionary<int, AccessRuleValue> TagsRules { get; private set; } = tagsRules ?? [];

	public UserAccessEntity? UnderlyingUser { get; private set; } = underlyingUser;

	public void AddUnderlyingUser(UserAccessEntity underlyingUser)
	{
		UnderlyingUser = underlyingUser;
	}

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
	public bool HasGlobalAccess(
		AccessType minimalAccess,
		bool withUnderlying = true)
	{
		bool hasAccess = RootRule.HasAccess(minimalAccess);

		if (hasAccess && withUnderlying && UnderlyingUser != null)
			hasAccess = UnderlyingUser.HasGlobalAccess(minimalAccess, false);

		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к источнику данных
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
	public bool HasAccessToSource(
		AccessType minimalAccess,
		int sourceId,
		bool withUnderlying = true)
	{
		var access = SourcesRules.TryGetValue(sourceId, out var rule) ? rule : RootRule;
		var hasAccess = access.HasAccess(minimalAccess);

		if (hasAccess && withUnderlying && UnderlyingUser != null)
			hasAccess = UnderlyingUser.HasAccessToSource(minimalAccess, sourceId, false);

		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к блоку
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
	public bool HasAccessToBlock(
		AccessType minimalAccess,
		int blockId,
		bool withUnderlying = true)
	{
		var access = BlocksRules.TryGetValue(blockId, out var rule) ? rule : RootRule;
		var hasAccess = access.HasAccess(minimalAccess);

		if (hasAccess && withUnderlying && UnderlyingUser != null)
			hasAccess = UnderlyingUser.HasAccessToBlock(minimalAccess, blockId, false);

		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к тегу
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
	public bool HasAccessToTag(
		AccessType minimalAccess,
		int tagId,
		bool withUnderlying = true)
	{
		var access = TagsRules.TryGetValue(tagId, out var rule) ? rule : RootRule;
		var hasAccess = access.HasAccess(minimalAccess);

		if (hasAccess && withUnderlying && UnderlyingUser != null)
			hasAccess = UnderlyingUser.HasAccessToTag(minimalAccess, tagId, false);

		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к группе пользователей
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
	public bool HasAccessToUserGroup(
		AccessType minimalAccess,
		Guid groupGuid,
		bool withUnderlying = true)
	{
		var access = GroupsRules.TryGetValue(groupGuid, out var rule) ? rule : RootRule;
		var hasAccess = access.HasAccess(minimalAccess);

		if (hasAccess && withUnderlying && UnderlyingUser != null)
			hasAccess = UnderlyingUser.HasAccessToUserGroup(minimalAccess, groupGuid, false);

		return hasAccess;
	}

	#endregion

	#region Ошибки

	/// <summary>
	/// Нет доступа для локального пользователя
	/// </summary>
	public static UnauthorizedException NoAccessUser(Guid userGuid)
		=> new("нет доступа.\nПользователь: [" + userGuid.ToString() + "]");

	/// <summary>
	/// Нет доступа для пользователя EnergoId через внешнего статичного пользователя
	/// </summary>
	public static UnauthorizedException NoAccessUnderlyingUser(Guid userGuid, Guid underlyingUserGuid)
		=> new("нет доступа.\nПользователь EnergoId: `" + underlyingUserGuid.ToString() + "` через внешнего пользователя `" + userGuid.ToString() + "`");


	/// <summary>
	/// Проверка достаточности глобального уровня доступа
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public void ThrowIfNoGlobalAccess(AccessType minimalAccess)
	{
		if (!HasGlobalAccess(minimalAccess, false))
			throw NoAccessUser(Guid);

		if (UnderlyingUser != null)
		{
			if (!UnderlyingUser.HasGlobalAccess(minimalAccess, false))
				throw NoAccessUnderlyingUser(Guid, UnderlyingUser.Guid);
		}
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к источнику данных
	/// </summary>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public void ThrowIfNoAccessToSource(AccessType minimalAccess, int sourceId)
	{
		if (!HasAccessToSource(minimalAccess, sourceId, false))
			throw NoAccessUser(Guid);

		if (UnderlyingUser != null)
		{
			if (!UnderlyingUser.HasAccessToSource(minimalAccess, sourceId, false))
				throw NoAccessUnderlyingUser(Guid, UnderlyingUser.Guid);
		}
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к блоку
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public void ThrowIfNoAccessToBlock(AccessType minimalAccess, int blockId)
	{
		if (!HasAccessToBlock(minimalAccess, blockId, false))
			throw NoAccessUser(Guid);

		if (UnderlyingUser != null)
		{
			if (!UnderlyingUser.HasAccessToBlock(minimalAccess, blockId, false))
				throw NoAccessUnderlyingUser(Guid, UnderlyingUser.Guid);
		}
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к тегу
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public void ThrowIfNoAccessToTag(AccessType minimalAccess, int tagId)
	{
		if (!HasAccessToTag(minimalAccess, tagId, false))
			throw NoAccessUser(Guid);

		if (UnderlyingUser != null)
		{
			if (!UnderlyingUser.HasAccessToTag(minimalAccess, tagId, false))
				throw NoAccessUnderlyingUser(Guid, UnderlyingUser.Guid);
		}
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к группе пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public void ThrowIfNoAccessToUserGroup(AccessType minimalAccess, Guid groupGuid)
	{
		if (!HasAccessToUserGroup(minimalAccess, groupGuid, false))
			throw NoAccessUser(Guid);

		if (UnderlyingUser != null)
		{
			if (!UnderlyingUser.HasAccessToUserGroup(minimalAccess, groupGuid, false))
				throw NoAccessUnderlyingUser(Guid, UnderlyingUser.Guid);
		}
	}

	#endregion

	#region Получение

	/// <summary>
	/// Получение правила доступа к источнику данных
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <returns>Правило доступа</returns>
	public AccessRuleValue GetAccessToSource(int sourceId)
	{
		if (UnderlyingUser == null)
		{
			return SourcesRules.TryGetValue(sourceId, out var rule) ? rule : RootRule;
		}
		else
		{
			return UnderlyingUser.SourcesRules.TryGetValue(sourceId, out var rule) ? rule : UnderlyingUser.RootRule;
		}
	}

	/// <summary>
	/// Получение правила доступа к блоку
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <returns>Правило доступа</returns>
	public AccessRuleValue GetAccessToBlock(int blockId)
	{
		if (UnderlyingUser == null)
		{
			return BlocksRules.TryGetValue(blockId, out var rule) ? rule : RootRule;
		}
		else
		{
			return UnderlyingUser.BlocksRules.TryGetValue(blockId, out var rule) ? rule : UnderlyingUser.RootRule;
		}
	}

	/// <summary>
	/// Получение правила доступа к тегу
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <returns>Правило доступа</returns>
	public AccessRuleValue GetAccessToTag(int tagId)
	{
		if (UnderlyingUser == null)
		{
			return TagsRules.TryGetValue(tagId, out var rule) ? rule : RootRule;
		}
		else
		{
			return UnderlyingUser.TagsRules.TryGetValue(tagId, out var rule) ? rule : UnderlyingUser.RootRule;
		}
	}

	/// <summary>
	/// Получение правила доступа к группе пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Правило доступа</returns>
	public AccessRuleValue GetAccessToUserGroup(Guid groupGuid)
	{
		if (UnderlyingUser == null)
		{
			return GroupsRules.TryGetValue(groupGuid, out var rule) ? rule : RootRule;
		}
		else
		{
			return UnderlyingUser.GroupsRules.TryGetValue(groupGuid, out var rule) ? rule : UnderlyingUser.RootRule;
		}
	}

	#endregion
}
