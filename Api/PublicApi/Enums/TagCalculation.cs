namespace Datalake.PublicApi.Enums;

/// <summary>
/// Способ вычисления значения
/// </summary>
public enum TagCalculation
{
	/// <summary>
	/// По формуле
	/// </summary>
	Formula = 1,

	/// <summary>
	/// По таблице пороговых значений
	/// </summary>
	Thresholds = 2,
}
