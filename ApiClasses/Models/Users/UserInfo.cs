using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Users;

/// <summary>
/// Информация о пользователе
/// </summary>
public class UserInfo
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Имя для входа
	/// </summary>
	public string? Login { get; set; }

	/// <summary>
	/// Полное имя
	/// </summary>
	public string? FullName { get; set; }

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
	public UserGroupsInfo[] UserGroups { get; set; } = [];
}
