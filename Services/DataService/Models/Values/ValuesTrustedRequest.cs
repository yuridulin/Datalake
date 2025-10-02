using Datalake.Contracts.Public.Enums;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Tags;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Values;

/// <summary>
/// Данные запроса для получения значений
/// </summary>
public class ValuesTrustedRequest
{
	/// <summary>
	/// Идентификатор запроса, который будет передан в соответствующий объект ответа
	/// </summary>
	[Required]
	public required string RequestKey { get; set; }

	/// <summary>
	/// Список кэшированной информации о выбранных тегах
	/// </summary>
	public required TagSettings[] Tags { get; set; } = [];

	/// <summary>
	/// Настройки времени
	/// </summary>
	[Required]
	public required TimeSettings Time { get; set; }

	/// <summary>
	/// Шаг времени, по которому нужно разбить значения. Если не задан, будут оставлены записи о изменениях значений
	/// </summary>
	public TagResolution? Resolution { get; set; } = TagResolution.NotSet;

	/// <summary>
	/// Тип агрегирования значений, который нужно применить к этому запросу. По умолчанию - список
	/// </summary>
	public AggregationFunc? Func { get; set; } = AggregationFunc.List;


	/// <summary>
	/// Настройки времени
	/// </summary>
	public record TimeSettings
	{
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
	}

	/// <summary>
	/// Промежуточные данные о тегах
	/// </summary>
	public class TagSettings : TagCacheInfo
	{
		/// <summary>
		/// Результат чтения (без учета ошибок)
		/// </summary>
		public ValueResult Result { get; set; }
	}
}
