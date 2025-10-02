using Datalake.PublicApi.Exceptions;

namespace Datalake.GatewayService;

public class Exceptions
{
	/// <summary>
	/// Нет доступа, так как нет сессии с таким токеном
	/// </summary>
	public static UnauthenticatedException NoAccessToken(string? token)
		=> new("NO_SESSION", "нет доступа.\nТокен: `" + (token ?? "не существует") + "`");


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

}
