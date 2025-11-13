using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Public.Models.Blocks;

/// <summary>
/// Базовая информация о блоке, достаточная, чтобы на него сослаться
/// </summary>
public class BlockSimpleInfo
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	[Required]
	public required int Id { get; set; } = 0;

	/// <summary>
	/// Глобальный идентификатор
	/// </summary>
	[Required]
	public required Guid Guid { get; set; }

	/// <summary>
	/// Наименование
	/// </summary>
	[Required]
	public required string Name { get; set; }
}
