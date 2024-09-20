using Datalake.ApiClasses.Models.Users;

namespace Datalake.Server.Services.SessionManager.Models;

/// <summary>
/// Данные сессии пользователя
/// </summary>
public class AuthSession
{
	/// <summary>
	/// Информация о пользователе
	/// </summary>
	public required UserAuthInfo User { get; set; }

	/// <summary>
	/// Время истечения сессии
	/// </summary>
	public required DateTime ExpirationTime { get; set; }

	/// <summary>
	/// Адрес, с которого разрешен доступ
	/// </summary>
	public string? StaticHost { get; set; }
}
