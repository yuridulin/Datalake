using Datalake.Contracts.Models.UserGroups;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Users;

/// <summary>
/// Информация о пользователе и группах, в которых он состоит
/// </summary>
public class UserWithGroupsInfo : UserInfo
{
	/// <summary>
	/// Список групп, в которые входит пользователь
	/// </summary>
	[Required]
	public IEnumerable<UserGroupSimpleInfo> UserGroups { get; set; } = [];
}
