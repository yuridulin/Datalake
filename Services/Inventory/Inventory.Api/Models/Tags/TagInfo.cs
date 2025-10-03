using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Api.Models.Abstractions;
using Datalake.Inventory.Api.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.Tags;

/// <summary>
/// Информация о теге
/// </summary>
public class TagInfo : TagSimpleInfo, IProtectedEntity
{
	/// <summary>
	/// Произвольное описание тега
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// Идентификатор источника данных
	/// </summary>
	[Required]
	public required int SourceId { get; set; }

	/// <summary>
	/// Путь к данным в источнике
	/// </summary>
	public string? SourceItem { get; set; }

	/// <summary>
	/// Имя используемого источника данных
	/// </summary>
	public string? SourceName { get; set; }

	/// <summary>
	/// Формула, на основе которой вычисляется значение
	/// </summary>
	public string? Formula { get; set; }

	/// <summary>
	/// Пороговые значения, по которым выбирается итоговое значение
	/// </summary>
	public List<TagThresholdInfo>? Thresholds { get; set; }

	/// <summary>
	/// Входной тег, по значениям которого выбирается значение из пороговой таблицы
	/// </summary>
	public TagAsInputInfo? ThresholdSourceTag { get; set; }

	/// <summary>
	/// Применяется ли приведение числового значения тега к другой шкале
	/// </summary>
	[Required]
	public required bool IsScaling { get; set; }

	/// <summary>
	/// Меньший предел итоговой шкалы
	/// </summary>
	[Required]
	public required float MinEu { get; set; }

	/// <summary>
	/// Больший предел итоговой шкалы
	/// </summary>
	[Required]
	public required float MaxEu { get; set; }

	/// <summary>
	/// Меньший предел шкалы исходного значения
	/// </summary>
	[Required]
	public required float MinRaw { get; set; }

	/// <summary>
	/// Больший предел шкалы исходного значения
	/// </summary>
	[Required]
	public required float MaxRaw { get; set; }

	/// <summary>
	/// Входные переменные для формулы, по которой рассчитывается значение
	/// </summary>
	[Required]
	public required TagInputInfo[] FormulaInputs { get; set; } = [];

	/// <summary>
	/// Тип агрегации
	/// </summary>
	public TagAggregation? Aggregation { get; set; }

	/// <summary>
	/// Временное окно для расчета агрегированного значения
	/// </summary>
	public TagResolution? AggregationPeriod { get; set; }

	/// <summary>
	/// Идентификатор тега, который будет источником данных для расчета агрегированного значения
	/// </summary>
	public TagAsInputInfo? SourceTag { get; set; }

	/// <inheritdoc />
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;
}
