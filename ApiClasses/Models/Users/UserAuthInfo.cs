using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Users;

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
	public required UserAccessRightsInfo[] Rights { get; set; }
}
