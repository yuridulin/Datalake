using Datalake.ApiClasses.Enums;

namespace Datalake.ApiClasses.Models.UserGroups;

/// <summary>
/// Информация о пользователей данной группы
/// </summary>
public class UserGroupUsersInfo
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public Guid Guid { get; set; }

	/// <summary>
	/// Уровень доступа пользователя в группе
	/// </summary>
	public AccessType AccessType { get; set; }

	/// <summary>
	/// Полное имя пользователя
	/// </summary>
	public string? FullName { get; set; }
}
