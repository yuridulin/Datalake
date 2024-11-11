using Datalake.Database.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Abstractions;

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
