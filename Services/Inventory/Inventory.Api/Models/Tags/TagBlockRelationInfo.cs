using Datalake.Inventory.Api.Models.Blocks;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.Tags;

/// <summary>
/// Краткая информация о блоке, имеющем связь с тегом, включая локальное имя тега в блоке
/// </summary>
public class TagBlockRelationInfo : BlockSimpleInfo
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
}
