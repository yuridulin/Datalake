using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Blocks;

/// <summary>
/// Информация о блоке
/// </summary>
public record BlockWithTagsInfo : BlockSimpleInfo
{
	/// <summary>
	/// Список прикреплённых тегов
	/// </summary>
	[Required]
	public IEnumerable<BlockNestedTagInfo> Tags { get; set; } = [];
}
