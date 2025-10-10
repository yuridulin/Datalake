using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Api.Models.UserGroups;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.Users;

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
	/// Тип учётной записи
	/// </summary>
	[Required]
	public UserType Type { get; set; }

	/// <summary>
	/// Идентификатор пользователя в сервере EnergoId
	/// </summary>
	public Guid? EnergoIdGuid { get; set; }

	/// <summary>
	/// Список групп, в которые входит пользователь
	/// </summary>
	[Required]
	public IEnumerable<UserGroupSimpleInfo> UserGroups { get; set; } = [];
}
