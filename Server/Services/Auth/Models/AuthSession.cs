using Datalake.PublicApi.Models.Auth;

namespace Datalake.Server.Services.Auth.Models;

/// <summary>
/// Данные сессии пользователя
/// </summary>
public class AuthSession
{
	/// <summary>
	/// Информация о пользователе
	/// </summary>
	public required Guid UserGuid { get; set; }

	/// <summary>
	/// Информация о правах пользователя
	/// </summary>
	public required UserAuthInfo AuthInfo { get; set; }

	/// <summary>
	/// Токен сессии
	/// </summary>
	public required string Token { get; set; }

	/// <summary>
	/// Время истечения сессии
	/// </summary>
	public required DateTime ExpirationTime { get; set; }

	/// <summary>
	/// Адрес, с которого разрешен доступ
	/// </summary>
	public string? StaticHost { get; set; }
}
