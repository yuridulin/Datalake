using Datalake.Contracts.Public.Models;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Public.Interfaces;

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
