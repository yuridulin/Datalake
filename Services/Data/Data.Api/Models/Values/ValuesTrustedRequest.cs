using Datalake.Contracts.Public.Enums;
using Datalake.Data.Api.Enums;
using System.ComponentModel.DataAnnotations;

namespace Datalake.Data.Api.Models.Values;

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
	public TagResolution? Resolution { get; set; } = TagResolution.None;

	/// <summary>
	/// Тип агрегирования значений, который нужно применить к этому запросу. По умолчанию - список
	/// </summary>
	public TagAggregation? Func { get; set; } = TagAggregation.None;


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
	public class TagSettings
	{
		/// <summary>
		/// Идентификатор тега в локальной базе
		/// </summary>
		[Required]
		public required int Id { get; set; }

		/// <summary>
		/// Глобальный идентификатор тега
		/// </summary>
		[Required]
		public required Guid Guid { get; set; }

		/// <summary>
		/// Имя тега
		/// </summary>
		[Required]
		public required string Name { get; set; }

		/// <summary>
		/// Тип данных тега
		/// </summary>
		[Required]
		public required TagType Type { get; set; }

		/// <summary>
		/// Частота записи тега
		/// </summary>
		[Required]
		public required TagResolution Resolution { get; set; }

		/// <summary>
		/// Тип данных источника
		/// </summary>
		[Required]
		public required SourceType SourceType { get; set; } = SourceType.Datalake;

		/// <summary>
		/// Коэффициент преобразования (соотношение новой и исходной шкал, заданных в настройках тега)
		/// </summary>
		[Required]
		public required float ScalingCoefficient { get; set; } = 1;

		/// <summary>
		/// Тег отмечен как удаленный
		/// </summary>
		[Required]
		public required bool IsDeleted { get; set; } = false;

		/// <summary>
		/// Идентификатор источника данных тега
		/// </summary>
		[Required]
		public required int SourceId { get; set; }
		/// <summary>
		/// Результат чтения (без учета ошибок)
		/// </summary>
		public ValueResult Result { get; set; }
	}
}
