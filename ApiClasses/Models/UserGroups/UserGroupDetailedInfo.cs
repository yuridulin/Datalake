using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.UserGroups;

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
	public required UserGroupInfo[] Subgroups { get; set; } = [];
}
