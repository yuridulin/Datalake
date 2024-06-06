using DatalakeDatabase.Models;
using DatalakeServer.BackgroundServices.Collector.Collectors.Abstractions;

namespace DatalakeServer.BackgroundServices.Collector.Collectors;

public class CalculateCollector(
	Source source,
	ILogger<CalculateCollector> logger) : CollectorBase(source, logger)
{
	public override event CollectEvent? CollectValues;

	public override Task Start()
	{
		CollectValues?.Invoke(this, []);
		return base.Start();
	}

	public override Task Stop()
	{
		return base.Stop();
	}
}
