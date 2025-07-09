using Datalake.PublicApi.Abstractions;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Blocks;

/// <summary>
/// Информация о закреплённом теге
/// </summary>
public class BlockNestedTagInfo : TagSimpleInfo, INestedItem
{
	/// <summary>
	/// Идентификатор связи
	/// </summary>
	[Required]
	public required int RelationId { get; set; }

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
