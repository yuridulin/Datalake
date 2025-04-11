using Datalake.PublicApi.Models.Metrics;
using System.Collections.Concurrent;

namespace Datalake.Database.Services;

/// <summary>
/// Сервис обработки метрик
/// </summary>
public static class MetricsService
{
	private static readonly ConcurrentQueue<HistoryReadMetric> _metricsQueue = new();

	/// <summary>
	/// Запуск обработки метрик
	/// </summary>
	static MetricsService()
	{
		Task.Run(ProcessQueueAsync);
	}

	/// <summary>
	/// Добавление метрики выполнения запроса на чтение
	/// </summary>
	/// <param name="metric"></param>
	public static void AddMetric(HistoryReadMetric metric)
	{
		_metricsQueue.Enqueue(metric);
	}

	/// <summary>
	/// Чтение текущих накопленных метрик по запросам чтения
	/// </summary>
	/// <returns>Список метрик</returns>
	public static HistoryReadMetric[] ReadMetrics()
	{
		return _metricsQueue.ToArray();
	}

	private static async Task ProcessQueueAsync()
	{
		const int maxSize = 1000;
		const int checkDelay = 100;

		while (true)
		{
			var currentCount = _metricsQueue.Count;

			if (currentCount > maxSize)
			{
				int itemsToRemove = currentCount - maxSize;
				TrimQueue(itemsToRemove);
			}

			await Task.Delay(checkDelay);
		}
	}

	private static void TrimQueue(int itemsToRemove)
	{
		while (itemsToRemove-- > 0)
		{
			_metricsQueue.TryDequeue(out _);
		}
	}
}
