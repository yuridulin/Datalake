using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.UserGroups;

/// <summary>
/// Информация о пользователей данной группы
/// </summary>
public class UserGroupUsersInfo
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	[Required]
	public Guid Guid { get; set; }

	/// <summary>
	/// Уровень доступа пользователя в группе
	/// </summary>
	[Required]
	public AccessType AccessType { get; set; }

	/// <summary>
	/// Полное имя пользователя
	/// </summary>
	public string? FullName { get; set; }
}
