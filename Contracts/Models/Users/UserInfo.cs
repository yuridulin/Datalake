using Datalake.Contracts.Models.UserGroups;
using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Users;

/// <summary>
/// Информация о пользователе
/// </summary>
public class UserInfo : UserSimpleInfo
{
	/// <summary>
	/// Имя для входа
	/// </summary>
	public string? Login { get; set; }

	/// <summary>
	/// Глобальный уровень доступа
	/// </summary>
	[Required]
	public required AccessType AccessType { get; set; }

	/// <summary>
	/// Идентификатор пользователя в сервере EnergoId
	/// </summary>
	public Guid? EnergoIdGuid { get; set; }
}

/// <summary>
/// Информация о пользователе и группах, в которых он состоит
/// </summary>
public class UserWithGroupsInfo : UserInfo
{
	/// <summary>
	/// Список групп, в которые входит пользователь
	/// </summary>
	[Required]
	public IEnumerable<UserGroupSimpleInfo> UserGroups { get; set; } = [];
}
