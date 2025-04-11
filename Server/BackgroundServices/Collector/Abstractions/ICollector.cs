using Datalake.Server.BackgroundServices.Collector.Models;

namespace Datalake.Server.BackgroundServices.Collector.Abstractions;

/// <summary>
/// Обработка собранных значений
/// </summary>
/// <param name="collector">Сборщик данных, значения которого собраны</param>
/// <param name="values">Список собранных значений</param>
public delegate void CollectEvent(ICollector collector, IEnumerable<CollectValue> values);

/// <summary>
/// Сборщик данных
/// </summary>
public interface ICollector
{
	/// <summary>
	/// Запуск сбора данных
	/// </summary>
	Task Start(CancellationToken stoppingToken);

	/// <summary>
	/// Прекращение сбора данных
	/// </summary>
	Task Stop();

	/// <summary>
	/// Событие получения набора новых значений
	/// </summary>
	event CollectEvent CollectValues;
}
