using System.Text.Json.Serialization;

namespace DatalakeApiClasses.Enums;

/// <summary>
/// Тип агрегирования данных
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum AggregationFunc
{
	/// <summary>
	/// Список без обработки
	/// </summary>
	List = 0,

	/// <summary>
	/// Сумма
	/// </summary>
	Sum = 1,

	/// <summary>
	/// Среднее арифметическое
	/// </summary>
	Avg = 2,

	/// <summary>
	/// Минимальное значение
	/// </summary>
	Min = 3,

	/// <summary>
	/// Максимальное значение
	/// </summary>
	Max = 4,
}
