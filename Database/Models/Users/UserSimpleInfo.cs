using Datalake.Database.Abstractions;
using Datalake.Database.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Users;

/// <summary>
/// Базовая информация о пользователе
/// </summary>
public class UserSimpleInfo : IProtectedEntity
{
	/// <summary>
	/// Идентификатор пользователя
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Имя пользователя
	/// </summary>
	[Required]
	public required string FullName { get; set; }

	/// <summary>
	/// Правило доступа
	/// </summary>
	[Required]
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
