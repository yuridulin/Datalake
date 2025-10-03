namespace Datalake.Contracts.Public.Enums;

/// <summary>
/// Способ получения агрегированного значения
/// </summary>
public enum TagAggregation
{
	/// <summary>
	/// Не используется
	/// </summary>
	None = 0,

	/// <summary>
	/// Сумма за прошедший период
	/// </summary>
	Sum = 1,

	/// <summary>
	/// Среднее за прошедший период
	/// </summary>
	Average = 2,

	/// <summary>
	/// Минимальное значение
	/// </summary>
	Min = 3,

	/// <summary>
	/// Максимальное значение
	/// </summary>
	Max = 4,
}
