using DatalakeApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace DatalakeApiClasses.Models.Values;

/// <summary>
/// Запись о значении
/// </summary>
public class ValueRecord
{
	/// <summary>
	/// Дата, на которую значение актуально
	/// </summary>
	[Required]
	public required DateTime Date { get; set; }

	/// <summary>
	/// Строковое представление даты
	/// </summary>
	[Required]
	public required string DateString { get; set; }

	/// <summary>
	/// Значение
	/// </summary>
	[Required]
	public required object? Value { get; set; }

	/// <summary>
	/// Достоверность значения
	/// </summary>
	[Required]
	public required TagQuality Quality { get; set; }

	/// <summary>
	/// Характеристика хранения значения
	/// </summary>
	[Required]
	public required TagUsing Using { get; set; }
}
