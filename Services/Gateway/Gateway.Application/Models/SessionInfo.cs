using Datalake.Contracts.Public.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Gateway.Application.Models;

/// <summary>
/// Данные сессии пользователя
/// </summary>
public record SessionInfo
{
	/// <summary>
	/// Токен сессии
	/// </summary>
	[Required]
	public required string Token { get; init; }

	/// <summary>
	/// Информация о пользователе
	/// </summary>
	[Required]
	public required Guid UserGuid { get; init; }

	/// <summary>
	/// Время истечения сессии
	/// </summary>
	[Required]
	public required DateTime ExpirationTime { get; init; }

	/// <summary>
	/// Тип входа в сессию. Нужен, чтобы правильно выйти
	/// </summary>
	[Required]
	public required UserType Type { get; init; }
}
