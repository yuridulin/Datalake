using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.UserGroups;

public class UpdateUserGroupRequest : CreateUserGroupRequest
{
	/// <summary>
	/// Базовый уровень доступа участников и под-групп
	/// </summary>
	public required AccessType AccessType { get; set; }

	/// <summary>
	/// Список пользователей, которые включены в эту группу
	/// </summary>
	[Required]
	public required UserGroupUsersInfo[] Users { get; set; } = [];

	/// <summary>
	/// Список групп, которые включены в эту группу
	/// </summary>
	[Required]
	public required UserGroupInfo[] Groups { get; set; } = [];
}
