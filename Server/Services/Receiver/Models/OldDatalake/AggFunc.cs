using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Datalake.Server.Services.Receiver.Models.OldDatalake;

/// <summary>
/// Тип агрегирующей функции
/// </summary>
[JsonConverter(typeof(StringEnumConverter))]
public enum AggFunc
{
	/// <summary>
	/// Список
	/// </summary>
	List = 0,
	/// <summary>
	/// Сумма значений
	/// </summary>
	Sum = 1,
	/// <summary>
	/// Среднее значение
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
