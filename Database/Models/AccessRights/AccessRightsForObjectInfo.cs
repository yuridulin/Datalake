using Datalake.Database.Models.UserGroups;
using Datalake.Database.Models.Users;

namespace Datalake.Database.Models.AccessRights;

/// <summary>
/// Информация о разрешении на объект для субьекта
/// </summary>
public class AccessRightsForObjectInfo : AccessRightsSimpleInfo
{
	/// <summary>
	/// Информация о группе пользователей
	/// </summary>
	public UserGroupSimpleInfo? UserGroup { get; set; }

	/// <summary>
	/// Информация о пользователе
	/// </summary>
	public UserSimpleInfo? User { get; set; }
}
