using System.Text.Json.Serialization;

namespace Datalake.ApiClasses.Enums;

/// <summary>
/// Характеристика значения
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum TagUsing
{
	/// <summary>
	/// Протянутое из прошлых таблиц значение, нужно для оптимизации чтения
	/// </summary>
	Initial = 0,

	/// <summary>
	/// Достоверное значение, полученное путем записи
	/// </summary>
	Basic = 1,

	/// <summary>
	/// Значение, полученное в результате математической операции над реальными значениями
	/// </summary>
	Aggregated = 2,

	/// <summary>
	/// Протянутое по времени значение, заполняющее пробелы между временными интервалами
	/// </summary>
	Continuous = 3,

	/// <summary>
	/// Устаревшее значение, которое было перезаписано
	/// </summary>
	Outdated = 100,

	/// <summary>
	/// Значение, которое не может существовать, потому что не было ни одной записи или инициализатора до него
	/// Нужно, что клиент знал, что запрошено несущствующее значение
	/// </summary>
	NotFound = 101,
}
