using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.UserGroups;

/// <summary>
/// Информация о принадлежности пользователя к группе в иерархическом представлении
/// </summary>
public class UserGroupsSimpleTreeInfo : UserGroupSimpleInfo
{
	/// <summary>
	/// Подгруппы этой группы
	/// </summary>
	[Required]
	public required UserGroupsSimpleTreeInfo[] Children { get; set; }
}
