using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

/// <summary>
/// Информация о принадлежности пользователя к группе
/// </summary>
public class UserGroupsInfo
{
	/// <summary>
	/// Идентификатор группы
	/// </summary>
	[Required]
	public required string Guid { get; set; }

	/// <summary>
	/// Имя группы
	/// </summary>
	[Required]
	public required string Name { get; set; }
}
