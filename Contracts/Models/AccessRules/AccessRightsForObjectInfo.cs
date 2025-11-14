using Datalake.Contracts.Models.UserGroups;
using Datalake.Contracts.Models.Users;

namespace Datalake.Contracts.Models.AccessRules;

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
