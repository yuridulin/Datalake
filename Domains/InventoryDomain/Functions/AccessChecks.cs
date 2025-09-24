using Datalake.Database.Constants;
using Datalake.Database.Extensions;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;

namespace Datalake.Database.Functions;

/// <summary>
/// Проверки уровней доступа
/// </summary>
public static class AccessChecks
{
	#region Проверки

	/// <summary>
	/// Проверка достаточности глобального уровня доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
	public static bool HasGlobalAccess(
		this UserAuthInfo user,
		AccessType minimalAccess,
		bool withUnderlying = true)
	{
		bool hasAccess = user.RootRule.HasAccess(minimalAccess);

		if (hasAccess && withUnderlying && user.UnderlyingUser != null)
			hasAccess = HasGlobalAccess(user.UnderlyingUser, minimalAccess);

		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к источнику данных
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
	public static bool HasAccessToSource(
		this UserAuthInfo user,
		AccessType minimalAccess,
		int sourceId,
		bool withUnderlying = true)
	{
		var access = user.Sources.TryGetValue(sourceId, out var rule) ? rule : user.RootRule;
		var hasAccess = access.HasAccess(minimalAccess);

		if (hasAccess && withUnderlying && user.UnderlyingUser != null)
			hasAccess = HasAccessToSource(user.UnderlyingUser, minimalAccess, sourceId);

		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к блоку
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
	public static bool HasAccessToBlock(
		this UserAuthInfo user,
		AccessType minimalAccess,
		int blockId,
		bool withUnderlying = true)
	{
		var access = user.Blocks.TryGetValue(blockId, out var rule) ? rule : user.RootRule;
		var hasAccess = access.HasAccess(minimalAccess);

		if (hasAccess && withUnderlying && user.UnderlyingUser != null)
			hasAccess = HasAccessToBlock(user.UnderlyingUser, minimalAccess, blockId);

		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к тегу
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
	public static bool HasAccessToTag(
		this UserAuthInfo user,
		AccessType minimalAccess,
		int tagId,
		bool withUnderlying = true)
	{
		var access = user.Tags.TryGetValue(tagId, out var rule) ? rule : user.RootRule;
		var hasAccess = access.HasAccess(minimalAccess);

		if (hasAccess && withUnderlying && user.UnderlyingUser != null)
			hasAccess = HasAccessToTag(user.UnderlyingUser, minimalAccess, tagId);

		return hasAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к группе пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <param name="withUnderlying">Проверять ли внутреннего пользователя</param>
	public static bool HasAccessToUserGroup(
		this UserAuthInfo user,
		AccessType minimalAccess,
		Guid groupGuid,
		bool withUnderlying = true)
	{
		var access = user.Groups.TryGetValue(groupGuid, out var rule) ? rule : user.RootRule;
		var hasAccess = access.HasAccess(minimalAccess);

		if (hasAccess && withUnderlying && user.UnderlyingUser != null)
			hasAccess = HasAccessToUserGroup(user.UnderlyingUser, minimalAccess, groupGuid);

		return hasAccess;
	}

	#endregion

	#region Ошибки

	/// <summary>
	/// Проверка достаточности глобального уровня доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoGlobalAccess(
		this UserAuthInfo user,
		AccessType minimalAccess)
	{
		if (!HasGlobalAccess(user, minimalAccess, false))
			throw Errors.NoAccessUser(user.Guid);

		if (user.UnderlyingUser != null)
		{
			if (!HasGlobalAccess(user.UnderlyingUser, minimalAccess, false))
				throw Errors.NoAccessUnderlyingUser(user.Guid, user.UnderlyingUser.Guid);
		}
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к источнику данных
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToSource(
		this UserAuthInfo user,
		AccessType minimalAccess,
		int sourceId)
	{
		if (!HasAccessToSource(user, minimalAccess, sourceId, false))
			throw Errors.NoAccessUser(user.Guid);

		if (user.UnderlyingUser != null)
		{
			if (!HasAccessToSource(user.UnderlyingUser, minimalAccess, sourceId, false))
				throw Errors.NoAccessUnderlyingUser(user.Guid, user.UnderlyingUser.Guid);
		}
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к блоку
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToBlock(
		this UserAuthInfo user,
		AccessType minimalAccess,
		int blockId)
	{
		if (!HasAccessToBlock(user, minimalAccess, blockId, false))
			throw Errors.NoAccessUser(user.Guid);

		if (user.UnderlyingUser != null)
		{
			if (!HasAccessToBlock(user.UnderlyingUser, minimalAccess, blockId, false))
				throw Errors.NoAccessUnderlyingUser(user.Guid, user.UnderlyingUser.Guid);
		}
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к тегу
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToTag(
		this UserAuthInfo user,
		AccessType minimalAccess,
		int tagId)
	{
		if (!HasAccessToTag(user, minimalAccess, tagId, false))
			throw Errors.NoAccessUser(user.Guid);

		if (user.UnderlyingUser != null)
		{
			if (!HasAccessToTag(user.UnderlyingUser, minimalAccess, tagId, false))
				throw Errors.NoAccessUnderlyingUser(user.Guid, user.UnderlyingUser.Guid);
		}
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к группе пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToUserGroup(
		this UserAuthInfo user,
		AccessType minimalAccess,
		Guid groupGuid)
	{
		if (!HasAccessToUserGroup(user, minimalAccess, groupGuid, false))
			throw Errors.NoAccessUser(user.Guid);

		if (user.UnderlyingUser != null)
		{
			if (!HasAccessToUserGroup(user.UnderlyingUser, minimalAccess, groupGuid, false))
				throw Errors.NoAccessUnderlyingUser(user.Guid, user.UnderlyingUser.Guid);
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
	public static AccessRuleInfo GetAccessToSource(
		this UserAuthInfo user,
		int sourceId)
	{
		if (user.UnderlyingUser == null)
		{
			return user.Sources.TryGetValue(sourceId, out var rule) ? rule : user.RootRule;
		}
		else
		{
			return user.UnderlyingUser.Sources.TryGetValue(sourceId, out var rule) ? rule : user.UnderlyingUser.RootRule;
		}
	}

	/// <summary>
	/// Получение правила доступа к блоку
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <returns>Правило доступа</returns>
	public static AccessRuleInfo GetAccessToBlock(
		this UserAuthInfo user,
		int blockId)
	{
		if (user.UnderlyingUser == null)
		{
			return user.Blocks.TryGetValue(blockId, out var rule) ? rule : user.RootRule;
		}
		else
		{
			return user.UnderlyingUser.Blocks.TryGetValue(blockId, out var rule) ? rule : user.UnderlyingUser.RootRule;
		}
	}

	/// <summary>
	/// Получение правила доступа к тегу
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <returns>Правило доступа</returns>
	public static AccessRuleInfo GetAccessToTag(
		this UserAuthInfo user,
		int tagId)
	{
		if (user.UnderlyingUser == null)
		{
			return user.Tags.TryGetValue(tagId, out var rule) ? rule : user.RootRule;
		}
		else
		{
			return user.UnderlyingUser.Tags.TryGetValue(tagId, out var rule) ? rule : user.UnderlyingUser.RootRule;
		}
	}

	/// <summary>
	/// Получение правила доступа к группе пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Правило доступа</returns>
	public static AccessRuleInfo GetAccessToUserGroup(
		this UserAuthInfo user,
		Guid groupGuid)
	{
		if (user.UnderlyingUser == null)
		{
			return user.Groups.TryGetValue(groupGuid, out var rule) ? rule : user.RootRule;
		}
		else
		{
			return user.UnderlyingUser.Groups.TryGetValue(groupGuid, out var rule) ? rule : user.UnderlyingUser.RootRule;
		}
	}

	#endregion
}
