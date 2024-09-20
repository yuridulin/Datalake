using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.UserGroups;

/// <summary>
/// Информация о группе пользователей
/// </summary>
public class UserGroupInfo
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

	/// <summary>
	/// Произвольное описание группы
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Идентификатор группы, в которой располагается эта группа
	/// </summary>
	public Guid? ParentGroupGuid { get; set; }
}
