using System.ComponentModel.DataAnnotations;

namespace Datalake.Gateway.Api.Models.Auth;

/// <summary>
/// Информация при аутентификации локальной учетной записи
/// </summary>
public record AuthLoginPassRequest
{
	/// <summary>
	/// Имя для входа
	/// </summary>
	[Required]
	public required string Login { get; init; }

	/// <summary>
	/// Пароль
	/// </summary>
	[Required]
	public required string Password { get; init; }
}
