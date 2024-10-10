using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Models.AccessRights;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.UserGroups;

/// <summary>
/// Расширенная информация о группе пользователей, включающая вложенные группы и список пользователей
/// </summary>
public class UserGroupDetailedInfo : UserGroupInfo
{
	/// <summary>
	/// Общий уровень доступа для всех участников группы
	/// </summary>
	[Required]
	public AccessType GlobalAccessType { get; set; }

	/// <summary>
	/// Список пользователей этой группы
	/// </summary>
	[Required]
	public required IEnumerable<UserGroupUsersInfo> Users { get; set; } = [];

	/// <summary>
	/// Список подгрупп этой группы
	/// </summary>
	[Required]
	public required IEnumerable<UserGroupSimpleInfo> Subgroups { get; set; } = [];

	/// <summary>
	/// Разрешения, выданные на эту группу
	/// </summary>
	[Required]
	public required IEnumerable<AccessRightsForOneInfo> AccessRights { get; set; } = [];
}
