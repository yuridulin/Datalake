using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Users;

/// <summary>
/// Информация о пользователе
/// </summary>
public class UserInfo : UserSimpleInfo
{
	/// <summary>
	/// Имя для входа
	/// </summary>
	public string? Login { get; set; }

	/// <summary>
	/// Адрес электронной почты
	/// </summary>
	public string? Email { get; set; }

	/// <summary>
	/// Глобальный уровень доступа
	/// </summary>
	[Required]
	public required AccessType AccessType { get; set; }
}
