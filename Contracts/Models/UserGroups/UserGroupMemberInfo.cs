using Datalake.Contracts.Models.Users;
using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.UserGroups;

/// <summary>
/// Информация о пользователей данной группы
/// </summary>
public class UserGroupMemberInfo : UserSimpleInfo
{
	/// <summary>
	/// Уровень доступа пользователя в группе
	/// </summary>
	[Required]
	public AccessType AccessType { get; set; }
}
