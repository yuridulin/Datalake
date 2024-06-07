using DatalakeApiClasses.Enums;

namespace DatalakeApiClasses.Models.Values;

/// <summary>
/// Данные запроса для получения значений
/// </summary>
public class ValuesRequest
{
	/// <summary>
	/// Список локальных идентификаторов тегов
	/// </summary>
	public int[]? Tags { get; set; } = [];

	/// <summary>
	/// Список имён тегов
	/// </summary>
	public string[]? TagNames { get; set; } = [];

	/// <summary>
	/// Дата, с которой (включительно) нужно получить значения. По умолчанию - начало текущих суток
	/// </summary>
	public DateTime? Old { get; set; }

	/// <summary>
	/// Дата, по которую (включительно) нужно получить значения. По умолчанию - текущая дата
	/// </summary>
	public DateTime? Young { get; set; }

	/// <summary>
	/// Дата, на которую (по точному соответствию) нужно получить значения. По умолчанию - не используется
	/// </summary>
	public DateTime? Exact { get; set; }

	/// <summary>
	/// Шаг времени, по которому нужно разбить значения. Если не задан, будут оставлены записи о изменениях значений
	/// </summary>
	public int? Resolution { get; set; } = 0;

	/// <summary>
	/// Тип агрегирования значений, который нужно применить к этому запросу. По умолчанию - список
	/// </summary>
	public AggregationFunc? Func { get; set; } = AggregationFunc.List;
}
