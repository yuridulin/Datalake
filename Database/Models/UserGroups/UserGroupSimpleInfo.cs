using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.UserGroups;

/// <summary>
/// Базовая информация о группе пользователей
/// </summary>
public class UserGroupSimpleInfo
{
	/// <summary>
	/// Идентификатор группы
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Название группы
	/// </summary>
	[Required]
	public required string Name { get; set; }
}
