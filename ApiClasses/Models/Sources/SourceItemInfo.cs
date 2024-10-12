using Datalake.ApiClasses.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.ApiClasses.Models.Sources;

/// <summary>
/// Информация о удалённой записи с данными источника
/// </summary>
public class SourceItemInfo
{
	/// <summary>
	/// Путь к данным в источнике
	/// </summary>
	[Required]
	public required string Path { get; set; }

	/// <summary>
	/// Тип данных
	/// </summary>
	[Required]
	public TagType Type { get; set; }

	/// <summary>
	/// Значение, прочитанное с источника при опросе
	/// </summary>
	public object? Value { get; set; }

	/// <summary>
	/// Достоверность значения
	/// </summary>
	[Required]
	public TagQuality Quality { get; set; } = TagQuality.Unknown;
}
