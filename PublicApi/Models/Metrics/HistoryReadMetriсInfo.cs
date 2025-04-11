using Datalake.PublicApi.Constants;
using System.ComponentModel.DataAnnotations;

namespace Datalake.PublicApi.Models.Metrics;

/// <summary>
/// Информация о результате выполнения запроса на чтение тегов
/// </summary>
public class HistoryReadMetricInfo
{
	/// <summary>
	/// Получение информации из результата
	/// </summary>
	/// <param name="metric">Результат</param>
	public HistoryReadMetricInfo(HistoryReadMetric metric)
	{
		TagsId = metric.TagsId;
		Sql = metric.Sql;
		RecordsCount = metric.RecordsCount;
		RequestKeys = metric.RequestKeys;

		Milliseconds = metric.Elapsed.TotalMilliseconds;
		Elapsed = DateFormats.FormatTimeSpan(metric.Elapsed);
		Date = metric.Date.ToString(DateFormats.Standart);

		if (metric.Old == metric.Young)
		{
			TimeSettings = $"на {metric.Old.ToString(DateFormats.Standart)}";
		}
		else if (metric.Old.Date == metric.Young.Date)
		{
			TimeSettings = $"на {metric.Old.ToString(DateFormats.Date)}: с {metric.Old.ToString(DateFormats.Time)} по {metric.Young.ToString(DateFormats.Time)}";
		}
		else
		{
			TimeSettings = $"с {metric.Old.ToString(DateFormats.Standart)} по {metric.Young.ToString(DateFormats.Standart)}";
		}
	}

	/// <summary>
	/// Время записи значения
	/// </summary>
	[Required]
	public string Date { get; set; }

	/// <summary>
	/// Идентификаторы тегов
	/// </summary>
	[Required]
	public int[] TagsId { get; set; }

	/// <summary>
	/// Настройки времени
	/// </summary>
	[Required]
	public string TimeSettings { get; set; }

	/// <summary>
	/// Время выполнения чтения
	/// </summary>
	[Required]
	public string Elapsed { get; set; }

	/// <summary>
	/// Прошедшее количество миллисекунд
	/// </summary>
	[Required]
	public double Milliseconds { get; set; }

	/// <summary>
	/// Итоговый SQL код запроса
	/// </summary>
	[Required]
	public string Sql { get; set; }

	/// <summary>
	/// Количество прочитанных из БД записей
	/// </summary>
	[Required]
	public int RecordsCount { get; set; }

	/// <summary>
	/// Список запросов к API, которые являются причиной запроса к БД
	/// </summary>
	[Required]
	public string[] RequestKeys { get; set; }
}
