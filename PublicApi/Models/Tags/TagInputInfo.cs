using Datalake.PublicApi.Abstractions;
using Datalake.PublicApi.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Tags;

/// <summary>
/// Тег, используемый как входной параметр в формуле
/// </summary>
public class TagInputInfo : TagSimpleInfo, IProtectedEntity
{
	/// <summary>
	/// Имя переменной, используемое в формуле
	/// </summary>
	[Required]
	public required string VariableName { get; set; }

	/// <summary>
	/// Идентификатор связи
	/// </summary>
	public int? RelationId { get; set; }

	/// <inheritdoc />
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
