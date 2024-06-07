using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

/// <summary>
/// Информация при аутентификации локальной учетной записи
/// </summary>
public class UserLoginPass
{
	/// <summary>
	/// Имя для входа
	/// </summary>
	[Required]
	public required string Login { get; set; }

	/// <summary>
	/// Пароль
	/// </summary>
	[Required]
	public required string Password { get; set; }
}
