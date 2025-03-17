using Datalake.PublicApi.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Abstractions;

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
