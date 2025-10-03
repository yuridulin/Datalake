using Datalake.Contracts.Public.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Datalake.PublicApi.Models.Values;

/// <summary>
/// Запись о значении
/// </summary>
public class ValueRecord
{
	/// <summary>
	/// Дата, на которую значение актуально
	/// </summary>
	[Required]
	public required DateTimeOffset Date { get; set; }

	/// <summary>
	/// Строковое представление даты
	/// </summary>
	[Required]
	public required string DateString { get; set; }

	/// <summary>
	/// Значение
	/// </summary>
	[AllowNull]
	public required object? Value { get; set; } = null;

	/// <summary>
	/// Достоверность значения
	/// </summary>
	[Required]
	public required TagQuality Quality { get; set; }
}
