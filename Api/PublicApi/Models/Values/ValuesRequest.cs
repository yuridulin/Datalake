using Datalake.PublicApi.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Values;

/// <summary>
/// Данные запроса для получения значений
/// </summary>
public class ValuesRequest
{
	/// <summary>
	/// Идентификатор запроса, который будет передан в соответствующий объект ответа
	/// </summary>
	[Required]
	public required string RequestKey { get; set; }

	/// <summary>
	/// Список глобальных идентификаторов тегов
	/// </summary>
	public Guid[]? Tags { get; set; } = [];

	/// <summary>
	/// Список локальных идентификаторов тегов
	/// </summary>
	public int[]? TagsId { get; set; }

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
	public TagResolution? Resolution { get; set; } = TagResolution.NotSet;

	/// <summary>
	/// Тип агрегирования значений, который нужно применить к этому запросу. По умолчанию - список
	/// </summary>
	public AggregationFunc? Func { get; set; } = AggregationFunc.List;
}
