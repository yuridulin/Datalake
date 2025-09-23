using Datalake.PublicApi.Models.Users;

namespace Datalake.PublicApi.Models.Tables;

/// <summary>
/// Информация о отношении пользователя к группе
/// </summary>
public class UserAccessGroupCTE : UserAccessGroupInfo
{
	/// <summary>
	/// Идентификатор группы
	/// </summary>
	public required Guid? ParentGuid { get; set; }

	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	public required Guid UserGuid { get; set; }
}
