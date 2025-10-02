namespace Datalake.Contracts.Public.Enums;

/// <summary>
/// Способ получения агрегированного значения
/// </summary>
public enum TagAggregation
{
	/// <summary>
	/// Сумма за прошедший период
	/// </summary>
	Sum = 1,

	/// <summary>
	/// Среднее за прошедший период
	/// </summary>
	Average = 2,
}
