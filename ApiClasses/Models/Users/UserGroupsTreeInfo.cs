using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Users;

/// <summary>
/// Информация о принадлежности пользователя к группе в иерархическом представлении
/// </summary>
public class UserGroupsTreeInfo : UserGroupsInfo
{
	/// <summary>
	/// Подгруппы этой группы
	/// </summary>
	[Required]
	public required UserGroupsTreeInfo[] Children { get; set; }
}
