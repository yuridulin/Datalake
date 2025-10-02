using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Inventory.Api.Models.Users;

namespace Datalake.Inventory.Api.Models.AccessRules;

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
