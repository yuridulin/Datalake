using Datalake.Contracts.Interfaces;
using Datalake.Contracts.Models.Tags;
using Datalake.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Models.Sources;

/// <summary>
/// Информация о теге, берущем данные из этого источника
/// </summary>
public class SourceTagInfo : TagSimpleInfo, IProtectedEntity
{
	public int SourceId { get; set; }

	/// <summary>
	/// Путь к данным в источнике
	/// </summary>
	[Required]
	public required string Item { get; set; }

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
	public TagInputMinimalInfo? ThresholdSourceTag { get; set; }

	/// <summary>
	/// Входные переменные для формулы, по которой рассчитывается значение
	/// </summary>
	[Required]
	public required TagInputMinimalInfo[] FormulaInputs { get; set; } = [];

	/// <summary>
	/// Входной тег, по значениям которого считается агрегированное значение
	/// </summary>
	public TagInputMinimalInfo? SourceTag { get; set; }

	/// <summary>
	/// Тип агрегации
	/// </summary>
	public TagAggregation? Aggregation { get; set; }

	/// <summary>
	/// Временное окно для расчета агрегированного значения
	/// </summary>
	public TagResolution? AggregationPeriod { get; set; }

	/// <inheritdoc />
	public AccessRuleInfo AccessRule { get; set; } = AccessRuleInfo.Default;

	/// <summary>
	/// Минимальная информация о переменных для расчета значений по формуле
	/// </summary>
	public class TagInputMinimalInfo
	{
		/// <summary>
		/// Идентификатор входного тега
		/// </summary>
		public required int InputTagId { get; set; }

		/// <summary>
		/// Тип данных входного тега
		/// </summary>
		public required TagType InputTagType { get; set; }

		/// <summary>
		/// Имя переменной
		/// </summary>
		public required string VariableName { get; set; }
	}
}
