using Datalake.PublicApi.Exceptions;

namespace Datalake.Database.Constants;

public static class Errors
{
	public static ForbiddenException NoAccess => new(message: "нет доступа");

	public static ForbiddenException NoAccessUser(Guid userGuid)
		=> new(message: "нет доступа.\nПользователь: [" + userGuid.ToString() + "]");

	public static ForbiddenException NoAccessUnderlyingUser(Guid userGuid, Guid underlyingUserGuid)
		=> new(message: "нет доступа.\nПользователь EnergoId: `" + userGuid.ToString() + "` через внешнего пользователя `" + underlyingUserGuid.ToString() + "`");

	public static ForbiddenException NoAccessToken(string? token)
		=> new(message: "нет доступа.\nТокен: `" + (token ?? "не существует") + "`");
}
