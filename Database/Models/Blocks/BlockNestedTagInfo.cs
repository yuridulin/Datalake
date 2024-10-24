using Datalake.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Blocks;

/// <summary>
/// Информация о закреплённом теге
/// </summary>
public class BlockNestedTagInfo : BlockNestedItem
{
	/// <summary>
	/// Идентификатор тега
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Тип поля блока для этого тега
	/// </summary>
	[Required]
	public BlockTagRelation Relation { get; set; } = BlockTagRelation.Static;

	/// <summary>
	/// Тип значений тега
	/// </summary>
	[Required]
	public TagType TagType { get; set; } = TagType.String;

	/// <summary>
	/// Свое имя тега в общем списке
	/// </summary>
	[Required]
	public string TagName { get; set; } = string.Empty;

	/// <summary>
	/// Идентификатор источника данных
	/// </summary>
	[Required]
	public int SourceId { get; set; }
}
