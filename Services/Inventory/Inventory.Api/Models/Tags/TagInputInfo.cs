using Datalake.Contracts.Public.Interfaces;
using Datalake.Inventory.Api.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.Tags;

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
	public int? BlockId { get; set; }

	/// <inheritdoc />
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
