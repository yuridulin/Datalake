using Datalake.Database.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Auth;

/// <summary>
/// Информация о аутентифицированном пользователе
/// </summary>
public class UserAuthInfo : UserSimpleInfo
{
	/// <summary>
	/// Идентификатор сессии
	/// </summary>
	[Required]
	public required string Token { get; set; }

	/// <summary>
	/// Список правил доступа
	/// </summary>
	[Required]
	public required UserRights Rights { get; set; }
}
