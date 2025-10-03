using Datalake.Data.Host.Models.Values;
using System.Collections.Concurrent;

namespace Datalake.Data.Host.Services.Metrics.Models;

/// <summary>
/// Объект метрики запроса на чтение данных
/// </summary>
public class ValuesRequestUsage : ValuesRequestUsageInfo
{
	// Потокобезопасная очередь меток времени вызовов
	private readonly ConcurrentQueue<DateTime> _calls = new();

	/// <summary>
	/// Добавление нового вызова запроса
	/// </summary>
	/// <param name="duration">Длительность</param>
	/// <param name="valuesCount">Количество значений</param>
	public void RecordCall(TimeSpan duration, int valuesCount)
	{
		var now = DateTime.UtcNow;
		LastExecutionTime = duration;
		LastExecutedAt = now;
		LastValuesCount = valuesCount;

		_calls.Enqueue(now);
		CleanupOld();
	}

	/// <summary>
	/// Подсчет количества запросов за последние сутки
	/// </summary>
	public override int RequestsLast24h
	{
		get
		{
			CleanupOld();
			return _calls.Count;
		}
	}

	// Удаляем записи старше 24 часов
	private void CleanupOld()
	{
		var threshold = DateTime.UtcNow.AddHours(-24);
		while (_calls.TryPeek(out var dt) && dt < threshold)
		{
			_calls.TryDequeue(out _);
		}
	}
}