using DatalakeApiClasses.Enums;

namespace DatalakeApiClasses.Models.Users;

/// <summary>
/// Информация о отношении пользователя к группе
/// </summary>
public class UserAccessGroupInfo
{
	/// <summary>
	/// Идентификатор группы
	/// </summary>
	public required Guid GroupGuid { get; set; }

	/// <summary>
	/// Уровень доступа к группе
	/// </summary>
	public required AccessType AccessType { get; set; }
}
