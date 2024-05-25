using System.ComponentModel.DataAnnotations;

namespace DatalakeDatabase.ApiModels.Blocks;

/// <summary>
/// Информация о сущности
/// </summary>
public class BlockSimpleInfo
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	[Required]
	public int Id { get; set; } = 0;

	/// <summary>
	/// Наименование
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Текстовое описание
	/// </summary>
	public string? Description { get; set; }
}
