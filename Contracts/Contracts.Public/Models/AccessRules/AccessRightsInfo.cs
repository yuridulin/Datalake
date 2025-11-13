using Datalake.Contracts.Public.Models.UserGroups;
using Datalake.Contracts.Public.Models.Users;

namespace Datalake.Contracts.Public.Models.AccessRules;

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
