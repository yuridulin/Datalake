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
	public int RuleId { get; } = RuleId;

	/// <summary>
	/// Уровень доступа
	/// </summary>
	[Required]
	public AccessType Access { get; } = Access;

	/// <summary>
	/// Заглушка для неопределенного уровня доступа
	/// </summary>
	public static readonly AccessRuleInfo Default = new(0, AccessType.NotSet);
}
