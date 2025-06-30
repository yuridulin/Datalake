using Datalake.PublicApi.Enums;
using System.ComponentModel.DataAnnotations;
using System.Runtime.InteropServices;

namespace Datalake.PublicApi.Models.Auth;

/// <summary>
/// Информация о уровне доступа с указанием на правило, на основе которого получен этот доступ
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public record AccessRuleInfo(int RuleId, AccessType Access)
{
	/// <summary>
	/// Идентификатор правила доступа
	/// </summary>
	[Required]
	public readonly int RuleId = RuleId;

	/// <summary>
	/// Уровень доступа
	/// </summary>
	[Required]
	public readonly AccessType Access = Access;

	/// <summary>
	/// Заглушка для неопределенного уровня доступа
	/// </summary>
	public static readonly AccessRuleInfo Default = new(0, AccessType.NotSet);
}
