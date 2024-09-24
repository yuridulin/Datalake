using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Blocks;

/// <summary>
/// Связанный с блоком объект
/// </summary>
public class BlockNestedItem
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	[Required]
	public int Id { get; set; }

	/// <summary>
	/// Наименование
	/// </summary>
	[Required]
	public required string Name { get; set; }
}
