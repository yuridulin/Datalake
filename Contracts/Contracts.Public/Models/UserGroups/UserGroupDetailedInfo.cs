using Datalake.Contracts.Public.Models.AccessRules;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Public.Models.UserGroups;

/// <summary>
/// Расширенная информация о группе пользователей, включающая вложенные группы и список пользователей
/// </summary>
public class UserGroupDetailedInfo : UserGroupInfo
{
	/// <summary>
	/// Список пользователей этой группы
	/// </summary>
	[Required]
	public required UserGroupUsersInfo[] Users { get; set; } = [];

	/// <summary>
	/// Список подгрупп этой группы
	/// </summary>
	[Required]
	public required UserGroupSimpleInfo[] Subgroups { get; set; } = [];

	/// <summary>
	/// Разрешения, выданные на эту группу
	/// </summary>
	[Required]
	public required AccessRightsForOneInfo[] AccessRights { get; set; } = [];
}
