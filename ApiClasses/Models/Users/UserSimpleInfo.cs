using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Users;

/// <summary>
/// Базовая информация о пользователе
/// </summary>
public class UserSimpleInfo
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
}
