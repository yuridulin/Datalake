using Datalake.PublicApi.Exceptions;

namespace Datalake.Database.Constants;

internal static class Errors
{
	internal static ForbiddenException NoAccess => new(message: "нет доступа");

	internal static ForbiddenException NoAccessUser(Guid userGuid)
		=> new(message: "нет доступа.\nПользователь: [" + userGuid.ToString() + "]");

	internal static ForbiddenException NoAccessUnderlyingUser(Guid userGuid, Guid underlyingUserGuid)
		=> new(message: "нет доступа.\nПользователь EnergoId: `" + underlyingUserGuid.ToString() + "` через внешнего пользователя `" + userGuid.ToString() + "`");
}
