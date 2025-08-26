using Datalake.PublicApi.Exceptions;

namespace Datalake.Database.Constants;

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
		=> new(message: "нет доступа.\nПользователь EnergoId: `" + userGuid.ToString() + "` через внешнего пользователя `" + underlyingUserGuid.ToString() + "`");

	/// <summary>
	/// Нет доступа, так как нет сессии с таким токеном
	/// </summary>
	public static ForbiddenException NoAccessToken(string? token)
		=> new(message: "нет доступа.\nТокен: `" + (token ?? "не существует") + "`");
}
