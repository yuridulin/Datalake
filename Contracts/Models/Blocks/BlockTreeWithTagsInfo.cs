using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Blocks;

/// <summary>
/// Информация о блоке
/// </summary>
public class BlockTreeWithTagsInfo : BlockTreeInfo
{
	/// <summary>
	/// Текстовое описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Список прикреплённых тегов
	/// </summary>
	[Required]
	public BlockNestedTagInfo[] Tags { get; set; } = [];
}
