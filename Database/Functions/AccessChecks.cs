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
	/// <summary>
	/// Проверка достаточности глобального уровня доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	public static bool HasGlobalAccess(
		UserAuthInfo user,
		AccessType minimalAccess)
	{
		bool access = user.RootRule.Access.HasAccess(minimalAccess);
		if (user.UnderlyingUser != null)
			access = access && HasGlobalAccess(user.UnderlyingUser, minimalAccess);

		return access;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к источнику данных
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="sourceId">Идентификатор источника</param>
	public static bool HasAccessToSource(
		UserAuthInfo user,
		AccessType minimalAccess,
		int sourceId)
	{
		if (!user.Sources.TryGetValue(sourceId, out var rule))
			return false;

		bool access = rule.Access.HasAccess(minimalAccess);
		if (user.UnderlyingUser != null)
			access = access && HasAccessToSource(user.UnderlyingUser, minimalAccess, sourceId);

		return access;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к блоку
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="blockId">Идентификатор блока</param>
	public static bool HasAccessToBlock(
		UserAuthInfo user,
		AccessType minimalAccess,
		int blockId)
	{
		if (!user.Blocks.TryGetValue(blockId, out var rule))
			return false;

		bool access = rule.Access.HasAccess(minimalAccess);
		if (user.UnderlyingUser != null)
			access = access && HasAccessToBlock(user.UnderlyingUser, minimalAccess, blockId);

		return access;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к тегу
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="id">Идентификатор тега</param>
	public static bool HasAccessToTag(
		UserAuthInfo user,
		AccessType minimalAccess,
		int id)
	{
		if (!user.Tags.TryGetValue(id, out var rule))
			return false;

		bool access = rule.Access.HasAccess(minimalAccess);
		if (user.UnderlyingUser != null)
			access = access && HasAccessToTag(user.UnderlyingUser, minimalAccess, id);

		return access;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к группе пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	public static bool HasAccessToUserGroup(
		UserAuthInfo user,
		AccessType minimalAccess,
		Guid groupGuid)
	{
		if (!user.Groups.TryGetValue(groupGuid, out var rule))
			return false;

		return rule.Access.HasAccess(minimalAccess);
	}

	/// <summary>
	/// Проверка достаточности глобального уровня доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoGlobalAccess(
		UserAuthInfo user,
		AccessType minimalAccess)
	{
		if (!HasGlobalAccess(user, minimalAccess))
			throw Errors.NoAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к источнику данных
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToSource(
		UserAuthInfo user,
		AccessType minimalAccess,
		int sourceId)
	{
		if (!HasAccessToSource(user, minimalAccess, sourceId))
			throw Errors.NoAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к блоку
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToBlock(
		UserAuthInfo user,
		AccessType minimalAccess,
		int blockId)
	{
		if (!HasAccessToBlock(user, minimalAccess, blockId))
			throw Errors.NoAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к тегу
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="id">Идентификатор тега</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToTag(
		UserAuthInfo user,
		AccessType minimalAccess,
		int id)
	{
		if (!HasAccessToTag(user, minimalAccess, id))
			throw Errors.NoAccess;
	}

	/// <summary>
	/// Проверка достаточности уровня доступа к группе пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="minimalAccess">Минимально необходимый уровень доступа</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <exception cref="ForbiddenException">Нет доступа</exception>
	public static void ThrowIfNoAccessToUserGroup(
		UserAuthInfo user,
		AccessType minimalAccess,
		Guid groupGuid)
	{
		if (!HasAccessToUserGroup(user, minimalAccess, groupGuid))
			throw Errors.NoAccess;
	}

	/// <summary>
	/// Получение глобального уровня доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Глобальный уровень доступа</returns>
	public static AccessType GetGlobalAccess(
		UserAuthInfo user)
	{
		return user.UnderlyingUser?.RootRule.Access ?? user.RootRule.Access;
	}

	/// <summary>
	/// Получение правила доступа к источнику данных
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <returns>Правило доступа</returns>
	public static AccessRuleInfo GetAccessToSource(
		UserAuthInfo user,
		int sourceId)
	{
		if (!user.Sources.TryGetValue(sourceId, out var rule))
			return AccessRuleInfo.Default;

		if (user.UnderlyingUser != null)
			if (!user.UnderlyingUser.Sources.TryGetValue(sourceId, out rule))
				return AccessRuleInfo.Default;

		return rule;
	}

	/// <summary>
	/// Получение правила доступа к блоку
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <returns>Правило доступа</returns>
	public static AccessRuleInfo GetAccessToBlock(
		UserAuthInfo user,
		int blockId)
	{
		if (!user.Blocks.TryGetValue(blockId, out var rule))
			return AccessRuleInfo.Default;

		if (user.UnderlyingUser != null)
			if (!user.UnderlyingUser.Blocks.TryGetValue(blockId, out rule))
				return AccessRuleInfo.Default;

		return rule;
	}

	/// <summary>
	/// Получение правила доступа к тегу
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор тега</param>
	/// <returns>Правило доступа</returns>
	public static AccessRuleInfo GetAccessToTag(
		UserAuthInfo user,
		int id)
	{
		if (!user.Tags.TryGetValue(id, out var rule))
			return AccessRuleInfo.Default;

		if (user.UnderlyingUser != null)
			if (!user.UnderlyingUser.Tags.TryGetValue(id, out rule))
				return AccessRuleInfo.Default;

		return rule;
	}

	/// <summary>
	/// Получение правила доступа к группе пользователей
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="groupGuid">Идентификатор группы</param>
	/// <returns>Правило доступа</returns>
	public static AccessRuleInfo GetAccessToUserGroup(
		UserAuthInfo user,
		Guid groupGuid)
	{
		if (!user.Groups.TryGetValue(groupGuid, out var rule))
			return AccessRuleInfo.Default;

		if (user.UnderlyingUser != null)
			if (!user.UnderlyingUser.Groups.TryGetValue(groupGuid, out rule))
				return AccessRuleInfo.Default;

		return rule;
	}
}
