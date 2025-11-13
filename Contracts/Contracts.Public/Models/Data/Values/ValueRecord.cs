using Datalake.Contracts.Public.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Datalake.Contracts.Public.Models.Data.Values;

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
	/// Текстовое значение
	/// </summary>
	[AllowNull]
	public required string? Text { get; set; } = null;

	/// <summary>
	/// Числовое значение
	/// </summary>
	[AllowNull]
	public required float? Number { get; set; } = null;

	/// <summary>
	/// Логическое значение
	/// </summary>
	[AllowNull]
	public required bool? Boolean { get; set; } = null;

	/// <summary>
	/// Достоверность значения
	/// </summary>
	[Required]
	public required TagQuality Quality { get; set; }
}
