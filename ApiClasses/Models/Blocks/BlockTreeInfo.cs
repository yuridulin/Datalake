using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Blocks;

/// <summary>
/// Информация о блоке в иерархическом представлении
/// </summary>
public class BlockTreeInfo : BlockWithTagsInfo
{
	/// <summary>
	/// Вложенные блоки
	/// </summary>
	[Required]
	public BlockTreeInfo[] Children { get; set; } = [];
}
