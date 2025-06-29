using Datalake.PublicApi.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Datalake.PublicApi.Models.Auth;

/// <summary>
/// Информация о уровне доступа с указанием на правило, на основе которого получен этот доступ
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly struct AccessRuleInfo(int ruleId, AccessType type)
{
	/// <summary>
	/// Идентификатор правила доступа
	/// </summary>
	[Required]
	public readonly int RuleId = ruleId;

	/// <summary>
	/// Уровень доступа
	/// </summary>
	[Required]
	public readonly AccessType AccessType = type;

	/// <summary>
	/// Заглушка для неопределенного уровня доступа
	/// </summary>
	public static readonly AccessRuleInfo Default = new(0, AccessType.NotSet);
}
