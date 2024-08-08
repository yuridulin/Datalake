using Datalake.ApiClasses.Enums;
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
	/// Имя сборщика данных
	/// </summary>
	public string Name { get; set; }

	/// <summary>
	/// Тип сборщика данных
	/// </summary>
	public SourceType Type { get; set; }

	/// <summary>
	/// Запуск сбора данных
	/// </summary>
	public Task Start();

	/// <summary>
	/// Прекращение сбора данных
	/// </summary>
	public Task Stop();

	/// <summary>
	/// Событие получения набора новых значений
	/// </summary>
	public event CollectEvent CollectValues;
}
