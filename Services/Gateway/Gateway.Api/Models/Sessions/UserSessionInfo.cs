using Datalake.Contracts.Public.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Gateway.Api.Models.Sessions;

/// <summary>
/// Данные сессии пользователя
/// </summary>
public class UserSessionInfo
{
	/// <summary>
	/// Информация о пользователе
	/// </summary>
	[Required]
	public required Guid UserGuid { get; set; }

	/// <summary>
	/// Информация о правах пользователя
	/// </summary>
	[Required]
	public UserAuthInfo AuthInfo { get; set; } = null!;

	/// <summary>
	/// Токен сессии
	/// </summary>
	[Required]
	public required string Token { get; set; }

	/// <summary>
	/// Время истечения сессии
	/// </summary>
	[Required]
	public required DateTime ExpirationTime { get; set; }

	/// <summary>
	/// Адрес, с которого разрешен доступ
	/// </summary>
	public string? StaticHost { get; set; }

	/// <summary>
	/// Тип входа в сессию. Нужен, чтобы правильно выйти
	/// </summary>
	[Required]
	public required UserType Type { get; set; }
}
