using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Users;

/// <summary>
/// Информация о принадлежности пользователя к группе
/// </summary>
public class UserGroupsInfo
{
	/// <summary>
	/// Идентификатор группы
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Имя группы
	/// </summary>
	[Required]
	public required string Name { get; set; }
}
