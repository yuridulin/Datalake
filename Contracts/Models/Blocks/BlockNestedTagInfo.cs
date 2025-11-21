using Datalake.Contracts.Models.Tags;
using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Blocks;

/// <summary>
/// Информация о закреплённом теге
/// </summary>
public class BlockNestedTagInfo
{
	/// <summary>
	/// Тип поля блока для этого тега
	/// </summary>
	[Required]
	public BlockTagRelation RelationType { get; set; } = BlockTagRelation.Static;

	/// <summary>
	/// Свое имя тега в общем списке
	/// </summary>
	public string? LocalName { get; set; }

	/// <summary>
	/// Используемый тег
	/// </summary>
	public TagSimpleInfo? Tag { get; set; }
	public int BlockId { get; set; }
	public int? TagId { get; set; }
}
