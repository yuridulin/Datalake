using DatalakeApiClasses.Enums;
using DatalakeDatabase.Models;

namespace DatalakeServer.BackgroundServices.Collector.Collectors.Abstractions;

/// <summary>
/// Базовый класс сборщика с реализацией основных механизмов
/// </summary>
/// <param name="source">Источник данных</param>
/// <param name="logger">Служба сообщений</param>
public abstract class CollectorBase(Source source, ILogger logger) : ICollector
{
	/// <inheritdoc />
	public string Name { get; set; } = source.Name;

	/// <inheritdoc />
	public SourceType Type { get; set; } = source.Type;

	/// <inheritdoc />
	public virtual Task Start()
	{
		logger.LogDebug("Start collect");
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public virtual Task Stop()
	{
		logger.LogDebug("Stop collect");
		return Task.CompletedTask;
	}

	/// <inheritdoc />
	public abstract event CollectEvent CollectValues;
}
