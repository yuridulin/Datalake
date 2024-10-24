using Datalake.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Users;

/// <summary>
/// Информация о отношении пользователя к группе
/// </summary>
public class UserAccessGroupInfo
{
	/// <summary>
	/// Идентификатор группы
	/// </summary>
	[Required]
	public required Guid GroupGuid { get; set; }

	/// <summary>
	/// Уровень доступа к группе
	/// </summary>
	[Required]
	public required AccessType AccessType { get; set; }
}
