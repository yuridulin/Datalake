using Datalake.PublicApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Auth;

/// <summary>
/// Информация о уровне доступа с указанием на правило, на основе которого получен этот доступ
/// </summary>
public class AccessRuleInfo
{
	/// <summary>
	/// Идентификатор правила доступа
	/// </summary>
	[Required]
	public int RuleId { get; set; }

	/// <summary>
	/// Уровень доступа
	/// </summary>
	[Required]
	public AccessType AccessType { get; set; }

	/// <summary>
	/// Заглушка для неопределенного уровня доступа
	/// </summary>
	public static readonly AccessRuleInfo Default = new() { RuleId = 0, AccessType = AccessType.NotSet };
}
