using Datalake.Database.Abstractions;
using Datalake.Database.Models.Auth;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Tags;

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

	/// <inheritdoc />
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
