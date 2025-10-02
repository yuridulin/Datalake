using Datalake.PublicApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Tags;

/// <summary>
/// Данные запроса для изменение тега
/// </summary>
public record TagUpdateRequest
{
	/// <summary>
	/// Новое имя тега
	/// </summary>
	[Required]
	public required string Name { get; init; }

	/// <summary>
	/// Новое описание
	/// </summary>
	public string? Description { get; init; }

	/// <summary>
	/// Новый тип данных
	/// </summary>
	[Required]
	public TagType Type { get; init; }

	/// <summary>
	/// Путь к данными в источнике
	/// </summary>
	public string? SourceItem { get; init; } = string.Empty;

	/// <summary>
	/// Применяется ли изменение шкалы значения
	/// </summary>
	[Required]
	public bool IsScaling { get; init; }

	/// <summary>
	/// Меньший предел итоговой шкалы
	/// </summary>
	[Required]
	public float MinEu { get; init; } = float.MinValue;

	/// <summary>
	/// Больший предел итоговой шкалы
	/// </summary>
	[Required]
	public float MaxEu { get; init; } = float.MaxValue;

	/// <summary>
	/// Меньший предел шкалы исходного значения
	/// </summary>
	[Required]
	public float MinRaw { get; init; } = float.MinValue;

	/// <summary>
	/// Больший предел шкалы исходного значения
	/// </summary>
	[Required]
	public float MaxRaw { get; init; } = float.MaxValue;

	/// <summary>
	/// Источник данных
	/// </summary>
	[Required]
	public int SourceId { get; init; } = (int)SourceType.Manual;

	/// <summary>
	/// Новый интервал получения значения
	/// </summary>
	[Required]
	public TagResolution Resolution { get; init; } = TagResolution.NotSet;

	/// <summary>
	/// Формула, на основе которой вычисляется значение
	/// </summary>
	public string? Formula { get; init; }

	/// <summary>
	/// Пороговые значения, по которым выбирается итоговое значение
	/// </summary>
	public IEnumerable<TagThresholdInfo> Thresholds { get; init; } = [];

	/// <summary>
	/// Идентификатор тега, который будет источником данных для выбора из пороговой таблицы
	/// </summary>
	public int? ThresholdSourceTagId { get; init; }

	/// <summary>
	/// Идентификатор связи, по которой выбран тег-источник данных для выбора из пороговой таблицы
	/// </summary>
	public int? ThresholdSourceTagBlockId { get; init; }

	/// <summary>
	/// Входные переменные для формулы, по которой рассчитывается значение
	/// </summary>
	[Required]
	public IEnumerable<TagUpdateInputRequest> FormulaInputs { get; init; } = [];

	/// <summary>
	/// Тип агрегации
	/// </summary>
	public TagAggregation? Aggregation { get; init; }

	/// <summary>
	/// Временное окно для расчета агрегированного значения
	/// </summary>
	public AggregationPeriod? AggregationPeriod { get; init; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета агрегированного значения
	/// </summary>
	public int? SourceTagId { get; init; }

	/// <summary>
	/// Идентификатор связи, по которой выбран тег-источник данных для расчета агрегированного значения
	/// </summary>
	public int? SourceTagBlockId { get; init; }
}
