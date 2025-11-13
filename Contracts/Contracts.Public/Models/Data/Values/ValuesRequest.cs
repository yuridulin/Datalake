using Datalake.Contracts.Public.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Contracts.Public.Models.Data.Values;

/// <summary>
/// Данные запроса для получения значений
/// </summary>
public record ValuesRequest
{
	/// <summary>
	/// Идентификатор запроса, который будет передан в соответствующий объект ответа
	/// </summary>
	[Required]
	public required string RequestKey { get; init; }

	/// <summary>
	/// Список локальных идентификаторов тегов
	/// </summary>
	public int[]? TagsId { get; init; }

	/// <summary>
	/// Список глобальных идентификаторов тегов
	/// </summary>
	public Guid[]? Tags { get; init; }

	/// <summary>
	/// Дата, с которой (включительно) нужно получить значения. По умолчанию - начало текущих суток
	/// </summary>
	public DateTime? Old { get; init; }

	/// <summary>
	/// Дата, по которую (включительно) нужно получить значения. По умолчанию - текущая дата
	/// </summary>
	public DateTime? Young { get; init; }

	/// <summary>
	/// Дата, на которую (по точному соответствию) нужно получить значения. По умолчанию - не используется
	/// </summary>
	public DateTime? Exact { get; init; }

	/// <summary>
	/// Шаг времени, по которому нужно разбить значения. Если не задан, будут оставлены записи о изменениях значений
	/// </summary>
	public TagResolution? Resolution { get; init; }

	/// <summary>
	/// Тип агрегирования значений, который нужно применить к этому запросу. По умолчанию - список
	/// </summary>
	public TagAggregation? Func { get; init; }
}
