using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

/// <summary>
/// Информация о аутентифицированном пользователе
/// </summary>
public class UserAuthInfo
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Имя пользователя
	/// </summary>
	[Required]
	public required string FullName { get; set; }

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
