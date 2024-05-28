using DatalakeApiClasses.Enums;
using DatalakeDatabase.Models;

namespace DatalakeServer.BackgroundServices.Collector.Collectors.Abstractions;

public abstract class CollectorBase(Source source, ILogger logger) : ICollector
{
	public string Name { get; set; } = source.Name;

	public SourceType Type { get; set; } = source.Type;

	public virtual Task Start()
	{
		logger.LogDebug("Start collect");
		return Task.CompletedTask;
	}

	public virtual Task Stop()
	{
		logger.LogDebug("Stop collect");
		return Task.CompletedTask;
	}

	public abstract event CollectEvent CollectValues;
}
