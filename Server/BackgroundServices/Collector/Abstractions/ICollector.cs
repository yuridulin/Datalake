using Datalake.PublicApi.Models.Values;
using System.Threading.Channels;

namespace Datalake.Server.BackgroundServices.Collector.Abstractions;

/// <summary>
/// Обработка собранных значений
/// </summary>
/// <param name="collector">Сборщик данных, значения которого собраны</param>
/// <param name="values">Список собранных значений</param>
public delegate void CollectEvent(ICollector collector, IEnumerable<ValueWriteRequest> values);

/// <summary>
/// Сборщик данных
/// </summary>
public interface ICollector
{
	/// <summary>
	/// Запуск сбора данных
	/// </summary>
	void Start(CancellationToken stoppingToken);

	/// <summary>
	/// Прекращение сбора данных
	/// </summary>
	void Stop();

	/// <summary>
	/// Имя сборщика
	/// </summary>
	string Name { get; }

	/// <summary>
	/// Выходной канал данных
	/// </summary>
	Channel<IEnumerable<ValueWriteRequest>> OutputChannel { get; }
}
