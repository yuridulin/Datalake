using Datalake.PrivateApi.ValueObjects;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;

namespace Datalake.PrivateApi.Entities;

public class UserAccessEntity(
	Guid guid,
	Guid? energoId,
	AccessRule rootRule,
	Dictionary<Guid, AccessRule> groupsRules,
	Dictionary<int, AccessRule> sourcesRules,
	Dictionary<int, AccessRule> blocksRules,
	Dictionary<int, AccessRule> tagsRules,
	UserAccessEntity? underlyingUser)
{
	public Guid Guid { get; private set; } = guid;

	public Guid? EnergoId { get; private set; } = energoId;

	public AccessRule RootRule { get; private set; } = rootRule;

	public Dictionary<Guid, AccessRule> GroupsRules { get; private set; } = groupsRules;

	public Dictionary<int, AccessRule> SourcesRules { get; private set; } = sourcesRules;

	public Dictionary<int, AccessRule> BlocksRules { get; private set; } = blocksRules;

	public Dictionary<int, AccessRule> TagsRules { get; private set; } = tagsRules;

	public UserAccessEntity? UnderlyingUser { get; private set; } = underlyingUser;

	public void AddUnderlyingUser(UserAccessEntity underlyingUser)
	{
		UnderlyingUser = underlyingUser;
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
	public static ForbiddenException NoAccessUser(Guid userGuid)
		=> new(message: "нет доступа.\nПользователь: [" + userGuid.ToString() + "]");

	/// <summary>
	/// Нет доступа для пользователя EnergoId через внешнего статичного пользователя
	/// </summary>
	public static ForbiddenException NoAccessUnderlyingUser(Guid userGuid, Guid underlyingUserGuid)
		=> new(message: "нет доступа.\nПользователь EnergoId: `" + underlyingUserGuid.ToString() + "` через внешнего пользователя `" + userGuid.ToString() + "`");


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
	public AccessRule GetAccessToSource(int sourceId)
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
	public AccessRule GetAccessToBlock(int blockId)
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
	public AccessRule GetAccessToTag(int tagId)
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
	public AccessRule GetAccessToUserGroup(Guid groupGuid)
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
