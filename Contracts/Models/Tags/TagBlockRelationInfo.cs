using Datalake.Contracts.Models.Blocks;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Tags;

/// <summary>
/// Краткая информация о блоке, имеющем связь с тегом, включая локальное имя тега в блоке
/// </summary>
public class TagBlockRelationInfo
{
	/// <summary>
	/// Идентификатор связи
	/// </summary>
	[Required]
	public required int RelationId { get; set; }

	/// <summary>
	/// Локальное имя тега в блоке
	/// </summary>
	public string? LocalName { get; set; }

	/// <summary>
	/// Связанный блок
	/// </summary>
	public BlockSimpleInfo? Block { get; set; }
}
