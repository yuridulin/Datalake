using Datalake.Contracts.Public.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Public.Models;

/// <summary>
/// Информация о уровне доступа с указанием на правило, на основе которого получен этот доступ
/// </summary>
public record AccessRuleInfo(int RuleId, AccessType Access)
{
	/// <summary>
	/// Идентификатор правила доступа
	/// </summary>
	[Required]
	public int RuleId { get; } = RuleId;

	/// <summary>
	/// Уровень доступа
	/// </summary>
	[Required]
	public AccessType Access { get; } = Access;

	/// <summary>
	/// Заглушка для неопределенного уровня доступа
	/// </summary>
	public static AccessRuleInfo Default { get; } = new(0, AccessType.None);
}
