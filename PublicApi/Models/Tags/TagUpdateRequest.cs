using Datalake.PublicApi.Abstractions;
using Datalake.PublicApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Tags;

/// <summary>
/// Данные запроса для изменение тега
/// </summary>
public class TagUpdateRequest : ICalculatedTag
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
	public TagResolution Resolution { get; set; } = TagResolution.NotSet;

	/// <summary>
	/// Используемый тип вычисления
	/// </summary>
	public TagCalculation? Calculation { get; set; }

	/// <summary>
	/// Формула, на основе которой вычисляется значение
	/// </summary>
	public string? Formula { get; set; }

	/// <summary>
	/// Пороговые значения, по которым выбирается итоговое значение
	/// </summary>
	public List<TagThresholdInfo>? Thresholds { get; set; }

	/// <summary>
	/// Входные переменные для формулы, по которой рассчитывается значение
	/// </summary>
	[Required]
	public TagUpdateInputRequest[] FormulaInputs { get; set; } = [];

	/// <summary>
	/// Тип агрегации
	/// </summary>
	public TagAggregation? Aggregation { get; set; }

	/// <summary>
	/// Временное окно для расчета агрегированного значения
	/// </summary>
	public AggregationPeriod? AggregationPeriod { get; set; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета агрегированного значения
	/// </summary>
	public int? SourceTagId { get; set; }

	/// <summary>
	/// Идентификатор связи, по которой выбран тег-источник данных для расчета агрегированного значения
	/// </summary>
	public int? SourceTagRelationId { get; set; }
}
