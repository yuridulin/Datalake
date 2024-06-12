using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Blocks;

/// <summary>
/// Информация о сущности в иерархическом представлении
/// </summary>
public class BlockTreeInfo
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	[Required]
	public int Id { get; set; }

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
	/// Вложенные сущности, подчинённые этой
	/// </summary>
	[Required]
	public BlockTreeInfo[] Children { get; set; } = [];
}
