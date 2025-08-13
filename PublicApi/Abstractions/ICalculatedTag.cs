using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;

namespace Datalake.PublicApi.Abstractions;

/// <summary>
/// Настройки вычисления
/// </summary>
public interface ICalculatedTag
{
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
}
