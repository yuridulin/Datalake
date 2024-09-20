using Datalake.ApiClasses.Enums;
using Datalake.Database.Models;

namespace Datalake.Server.BackgroundServices.Collector.Abstractions;

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
	public virtual Task Start(CancellationToken stoppingToken)
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
