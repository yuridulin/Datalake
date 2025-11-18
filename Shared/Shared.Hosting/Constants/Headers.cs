namespace Datalake.Shared.Hosting.Constants;

/// <summary>
/// Заголовки, используемые для общения между сервисами и шлюзом
/// </summary>
public static class Headers
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public static string UserGuidHeader { get; } = "X-Forwarded-User";

	/// <summary>
	/// Сессионный токен
	/// </summary>
	public static string SessionTokenHeander { get; } = "X-Session-Token";
}
