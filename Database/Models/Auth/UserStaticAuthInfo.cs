using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Auth;

/// <summary>
/// Информация о статической учетной записи
/// </summary>
public class UserStaticAuthInfo
{
	/// <summary>
	/// Идентификатор учетной записи
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Адрес, с которого разрешен доступ.
	/// Если пустой, адрес не проверяется
	/// </summary>
	public string? Host { get; set; }

	/// <summary>
	/// Информация о аутентифицированном пользователе
	/// </summary>
	[Required]
	public required UserAuthInfo AuthInfo { get; set; }
}
