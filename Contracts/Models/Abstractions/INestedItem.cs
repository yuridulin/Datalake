using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Abstractions;

/// <summary>
/// Связанный объект
/// </summary>
public interface INestedItem
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
	public string Name { get; set; }
}
