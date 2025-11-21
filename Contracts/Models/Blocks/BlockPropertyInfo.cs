using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Blocks;

/// <summary>
/// Информация о статическом свойстве блока
/// </summary>
public class BlockPropertyInfo
{
	/// <summary>
	/// Идентификатор
	/// </summary>
	public int Id { get; set; }

	/// <summary>
	/// Название свойства
	/// </summary>
	public required string Name { get; set; }

	/// <summary>
	/// Тип значения свойства
	/// </summary>
	[Required]
	public required TagType Type { get; set; }

	/// <summary>
	/// Значение свойства
	/// </summary>
	[Required]
	public required string Value { get; set; }
}
