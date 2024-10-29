using Datalake.Database.Enums;

namespace Datalake.Database.Models.Auth;

/// <summary>
/// Информация о уровне доступа с указанием на правило, на основе которого получен этот доступ
/// </summary>
public class AccessRule
{
	/// <summary>
	/// Идентификатор правила доступа
	/// </summary>
	public int RuleId { get; set; }

	/// <summary>
	/// Уровень доступа
	/// </summary>
	public AccessType AccessType { get; set; }
}
