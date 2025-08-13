namespace Datalake.PublicApi.Enums;

/// <summary>
/// Период, за который берутся необходимые для расчета агрегированных значений данные
/// </summary>
public enum AggregationPeriod
{
	/// <summary>
	/// Минута
	/// </summary>
	Minute = 1,

	/// <summary>
	/// Час
	/// </summary>
	Hour = 2,

	/// <summary>
	/// Сутки
	/// </summary>
	Day = 3,
}
