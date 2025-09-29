using Datalake.PublicApi.Exceptions;

namespace Datalake.InventoryService.Application.Constants;

/// <summary>
/// Ошибки
/// </summary>
public static class Errors
{
	/// <summary>
	/// Нет доступа на выполнение действия
	/// </summary>
	public static ForbiddenException NoAccess => new(message: "нет доступа");

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
	/// Нет доступа, так как нет сессии с таким токеном
	/// </summary>
	public static ForbiddenException NoAccessToken(string? token)
		=> new(message: "нет доступа.\nТокен: `" + (token ?? "не существует") + "`");

	public static NotFoundException NotFoundBlock(int blockId) => new($"Блок с идентификатором {blockId} не найден");
	public static NotFoundException NotFoundSource(int sourceId) => new($"Источник с идентификатором {sourceId} не найден");
	public static NotFoundException NotFoundTag(int tagId) => new($"Тег с идентификатором {tagId} не найден");
	public static NotFoundException NotFoundUser(Guid userGuid) => new($"Учетная запись с идентификатором {userGuid} не найдена");
	public static NotFoundException NotFoundUserGroup(Guid userGroupGuid) => new($"Группа учетных записей с идентификатором {userGroupGuid} не найдена");
}
