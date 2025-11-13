using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Public.Models.UserGroups;

/// <summary>
/// Информация о группе пользователей в иерархическом представлении
/// </summary>
public class UserGroupTreeInfo : UserGroupInfo
{
	/// <summary>
	/// Список подгрупп
	/// </summary>
	[Required]
	public UserGroupTreeInfo[] Children { get; set; } = [];

	/// <summary>
	/// Идентификатор родительской группы
	/// </summary>
	public Guid? ParentGuid { get; set; }

	/// <summary>
	/// Информация о родительской группе
	/// </summary>
	public UserGroupTreeInfo? Parent { get; set; }
}
