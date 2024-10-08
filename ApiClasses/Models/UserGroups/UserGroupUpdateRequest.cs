using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.UserGroups;

/// <summary>
/// Данные запроса для изменения группы пользователей
/// </summary>
public class UserGroupUpdateRequest : UserGroupCreateRequest
{
	/// <summary>
	/// Базовый уровень доступа участников и под-групп
	/// </summary>
	[Required]
	public required AccessType AccessType { get; set; }

	/// <summary>
	/// Список пользователей, которые включены в эту группу
	/// </summary>
	[Required]
	public required UserGroupUsersInfo[] Users { get; set; } = [];
}
