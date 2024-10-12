using Datalake.ApiClasses.Models.UserGroups;
using Datalake.ApiClasses.Models.Users;

namespace Datalake.ApiClasses.Models.AccessRights;

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
