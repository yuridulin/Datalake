using Datalake.Contracts.Public.Enums;
using Datalake.Contracts.Public.Interfaces;
using Datalake.Contracts.Public.Models;
using Datalake.Inventory.Api.Models.Tags;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Inventory.Api.Models.Sources;

/// <summary>
/// Информация о теге, берущем данные из этого источника
/// </summary>
public class SourceTagInfo : TagSimpleInfo, IProtectedEntity
{
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
