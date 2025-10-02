using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Api.Models.Abstractions;
using Datalake.Inventory.Api.Models.Tags;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.Blocks;

/// <summary>
/// Информация о закреплённом теге
/// </summary>
public class BlockNestedTagInfo : TagSimpleInfo, INestedItem
{
	/// <summary>
	/// Тип поля блока для этого тега
	/// </summary>
	[Required]
	public BlockTagRelation RelationType { get; set; } = BlockTagRelation.Static;

	/// <summary>
	/// Свое имя тега в общем списке
	/// </summary>
	[Required]
	public string LocalName { get; set; } = string.Empty;

	/// <summary>
	/// Идентификатор источника данных
	/// </summary>
	[Required]
	public int SourceId { get; set; }
}
