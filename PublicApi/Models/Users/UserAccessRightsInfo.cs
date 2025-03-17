using Datalake.PublicApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Users;

/// <summary>
/// Правило доступа пользователя
/// </summary>
public class UserAccessRightsInfo
{
	/// <summary>
	/// Является ли это правило глобальным
	/// </summary>
	[Required]
	public required bool IsGlobal { get; set; }

	/// <summary>
	/// Идентификатор тега, на который распространяется это правило
	/// </summary>
	public int? TagId { get; set; }

	/// <summary>
	/// Идентификатор источника, на который распространяется это правило
	/// </summary>
	public int? SourceId { get; set; }

	/// <summary>
	/// Идентификатор блока, на который распространяется это правило
	/// </summary>
	public int? BlockId { get; set; }

	/// <summary>
	/// Уровень доступа на основе этого правила
	/// </summary>
	[Required]
	public required AccessType AccessType { get; set; } = AccessType.NoAccess;
}
