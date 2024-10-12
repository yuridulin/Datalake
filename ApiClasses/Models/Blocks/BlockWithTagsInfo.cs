using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Blocks;

/// <summary>
/// Информация о блоке
/// </summary>
public class BlockWithTagsInfo : BlockSimpleInfo
{
	/// <summary>
	/// Идентификатор родительского блока
	/// </summary>
	public int? ParentId { get; set; }

	/// <summary>
	/// Текстовое описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Список прикреплённых тегов
	/// </summary>
	[Required]
	public IEnumerable<BlockNestedTagInfo> Tags { get; set; } = [];
}
