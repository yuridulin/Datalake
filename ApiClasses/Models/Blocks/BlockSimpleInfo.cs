using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Blocks;

/// <summary>
/// Информация о блоке
/// </summary>
public class BlockSimpleInfo
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	[Required]
	public int Id { get; set; } = 0;

	/// <summary>
	/// Идентификатор родительского блока
	/// </summary>
	public int? ParentId { get; set; }

	/// <summary>
	/// Наименование
	/// </summary>
	[Required]
	public required string Name { get; set; }

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
