using Datalake.PublicApi.Abstractions;
using Datalake.PublicApi.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.UserGroups;

/// <summary>
/// Базовая информация о группе пользователей
/// </summary>
public class UserGroupSimpleInfo : IProtectedEntity
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
	/// Правило доступа
	/// </summary>
	[Required]
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
