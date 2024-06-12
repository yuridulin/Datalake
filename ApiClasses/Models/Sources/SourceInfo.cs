using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Sources;

/// <summary>
/// Информация о источнике
/// </summary>
public class SourceInfo
{
	/// <summary>
	/// Идентификатор источника в базе данных
	/// </summary>
	[Required]
	public int Id { get; set; }

	/// <summary>
	/// Название источника
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Произвольное описание источника
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Используемый для получения данных адрес
	/// </summary>
	public string? Address { get; set; }

	/// <summary>
	/// Тип протокола, по которому запрашиваются данные
	/// </summary>
	[Required]
	public SourceType Type { get; set; }
}
