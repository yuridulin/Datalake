using Datalake.PrivateApi.Exceptions;

namespace Datalake.InventoryService.Application.Constants;

/// <summary>
/// Ошибки
/// </summary>
public static class Errors
{
	/// <summary>
	/// Нет доступа на выполнение действия
	/// </summary>
	public static UnauthorizedException NoAccess => new("NO_ACCESS", "нет доступа");

	/// <summary>
	/// Нет доступа для локального пользователя
	/// </summary>
	public static UnauthorizedException NoAccessUser(Guid userGuid)
		=> new("NO_ACCESS", "нет доступа.\nПользователь: [" + userGuid.ToString() + "]");

	/// <summary>
	/// Нет доступа для пользователя EnergoId через внешнего статичного пользователя
	/// </summary>
	public static UnauthorizedException NoAccessUnderlyingUser(Guid userGuid, Guid underlyingUserGuid)
		=> new("NO_ACCESS", "нет доступа.\nПользователь EnergoId: `" + underlyingUserGuid.ToString() + "` через внешнего пользователя `" + userGuid.ToString() + "`");

	/// <summary>
	/// Нет доступа, так как нет сессии с таким токеном
	/// </summary>
	public static UnauthenticatedException NoAccessToken(string? token)
		=> new("NO_SESSION", "нет доступа.\nТокен: `" + (token ?? "не существует") + "`");

	public static NotFoundException NotFoundBlock(int blockId)
	{
		return new("BLOCK_NOT_FOUND", $"Блок с идентификатором {blockId} не найден");
	}

	public static NotFoundException NotFoundBlock(string message)
	{
		return new("BLOCK_NOT_FOUND", message);
	}

	public static NotFoundException NotFoundSource(int sourceId) => new("SOURCE_NOT_FOUND", $"Источник с идентификатором {sourceId} не найден");
	public static NotFoundException NotFoundTag(int tagId, string? details = null)
		=> new("TAG_NOT_FOUND", $"Тег с идентификатором {tagId} не найден" + (string.IsNullOrEmpty(details) ? string.Empty : $". {details}"));
	public static NotFoundException NotFoundUser(Guid userGuid) => new("USER_NOT_FOUND", $"Учетная запись с идентификатором {userGuid} не найдена");
	public static NotFoundException NotFoundUserGroup(Guid userGroupGuid) => new("USER_GROUP_NOT_FOUND", $"Группа учетных записей с идентификатором {userGroupGuid} не найдена");
}
