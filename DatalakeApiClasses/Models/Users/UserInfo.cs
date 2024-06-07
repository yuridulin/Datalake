using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Users;

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
	[Required]
	public required string Login { get; set; }

	/// <summary>
	/// Полное имя
	/// </summary>
	public string? FullName { get; set; }

	/// <summary>
	/// Глобальные уровень доступа
	/// </summary>
	[Required]
	public required AccessType AccessType { get; set; }

	/// <summary>
	/// Является ли учётная запись статичной
	/// </summary>
	[Required]
	public required bool IsStatic { get; set; }

	/// <summary>
	/// Список групп, в которые входит пользователь
	/// </summary>
	[Required]
	public UserGroupsInfo[] UserGroups { get; set; } = [];
}
