using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Blocks;

/// <summary>
/// Информация о блоке в иерархическом представлении
/// </summary>
public record BlockTreeInfo : BlockWithTagsInfo
{
	/// <summary>
	/// Вложенные блоки
	/// </summary>
	[Required]
	public required IEnumerable<BlockTreeInfo> Children { get; set; }
}
