namespace Data.Api.Models.Values;

/// <summary>
/// Метрика запроса на чтение данных
/// </summary>
public class ValuesRequestUsageInfo
{
	/// <summary>
	/// Время последнего выполнения
	/// </summary>
	public TimeSpan LastExecutionTime { get; protected set; }

	/// <summary>
	/// Время последнего завершения выполнения
	/// </summary>
	public DateTime LastExecutedAt { get; protected set; }

	/// <summary>
	/// Количество значений в последнем запросе
	/// </summary>
	public int LastValuesCount { get; protected set; }

	/// <summary>
	/// Подсчет количества запросов за последние сутки
	/// </summary>
	public virtual int RequestsLast24h { get; }
}