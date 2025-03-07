using Datalake.Database.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Database.Models.Tags;

/// <summary>
/// Данные запроса для изменение тега
/// </summary>
public class TagUpdateRequest
{
	/// <summary>
	/// Новое имя тега
	/// </summary>
	[Required]
	public required string Name { get; set; }

	/// <summary>
	/// Новое описание
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Новый тип данных
	/// </summary>
	[Required]
	public TagType Type { get; set; }

	/// <summary>
	/// Путь к данными в источнике
	/// </summary>
	public string? SourceItem { get; set; } = string.Empty;

	/// <summary>
	/// Применяется ли изменение шкалы значения
	/// </summary>
	[Required]
	public bool IsScaling { get; set; }

	/// <summary>
	/// Меньший предел итоговой шкалы
	/// </summary>
	[Required]
	public float MinEu { get; set; } = float.MinValue;

	/// <summary>
	/// Больший предел итоговой шкалы
	/// </summary>
	[Required]
	public float MaxEu { get; set; } = float.MaxValue;

	/// <summary>
	/// Меньший предел шкалы исходного значения
	/// </summary>
	[Required]
	public float MinRaw { get; set; } = float.MinValue;

	/// <summary>
	/// Больший предел шкалы исходного значения
	/// </summary>
	[Required]
	public float MaxRaw { get; set; } = float.MaxValue;

	/// <summary>
	/// Источник данных
	/// </summary>
	[Required]
	public int SourceId { get; set; } = (int)SourceType.Manual;

	/// <summary>
	/// Новый интервал получения значения
	/// </summary>
	[Required]
	public TagFrequency Frequency { get; set; } = TagFrequency.NotSet;

	/// <summary>
	/// Формула, по которой рассчитывается значение
	/// </summary>
	public string? Formula { get; set; } = string.Empty;

	/// <summary>
	/// Входные переменные для формулы, по которой рассчитывается значение
	/// </summary>
	[Required]
	public TagUpdateInputRequest[] FormulaInputs { get; set; } = [];
}
