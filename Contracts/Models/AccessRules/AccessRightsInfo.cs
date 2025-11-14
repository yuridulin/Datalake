using Datalake.Contracts.Models.UserGroups;
using Datalake.Contracts.Models.Users;

namespace Datalake.Contracts.Models.AccessRules;

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
