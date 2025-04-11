using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Metrics;

/// <summary>
/// Результат выполнения запроса на чтение тегов
/// </summary>
public class HistoryReadMetric
{
	/// <summary>
	/// Время записи значения
	/// </summary>
	[Required]
	public required DateTime Date { get; set; }

	/// <summary>
	/// Идентификаторы тегов
	/// </summary>
	[Required]
	public required int[] TagsId { get; set; }

	/// <summary>
	/// Дата начала диапазона
	/// </summary>
	[Required]
	public required DateTime Old { get; set; }

	/// <summary>
	/// Дата конца диапазона
	/// </summary>
	[Required]
	public required DateTime Young { get; set; }

	/// <summary>
	/// Время выполнения чтения
	/// </summary>
	[Required]
	public required TimeSpan Elapsed { get; set; }

	/// <summary>
	/// Итоговый SQL код запроса
	/// </summary>
	[Required]
	public required string Sql { get; set; }

	/// <summary>
	/// Количество прочитанных из БД записей
	/// </summary>
	[Required]
	public required int RecordsCount { get; set; }
}
