using Datalake.Inventory.Api.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.Abstractions;

/// <summary>
/// Объект, доступ к которому проверяется по системе правил
/// </summary>
public interface IProtectedEntity
{
	/// <summary>
	/// Правило доступа
	/// </summary>
	[Required]
	public AccessRuleInfo AccessRule { get; set; }
}
