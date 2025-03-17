using Datalake.PublicApi.Models.UserGroups;
using Datalake.PublicApi.Models.Users;

namespace Datalake.PublicApi.Models.AccessRights;

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
