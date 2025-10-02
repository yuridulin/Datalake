using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Inventory.Api.Models.Users;

namespace Datalake.Inventory.Api.Models.AccessRules;

/// <summary>
/// Информация о разрешении пользователя или группы на доступ к какому-либо объекту
/// </summary>
public class AccessRightsInfo : AccessRightsForOneInfo
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
