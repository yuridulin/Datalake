using DatalakeApp.BackgroundServices.Collector.Collectors.Abstractions;
using DatalakeDatabase.Models;

namespace DatalakeApp.BackgroundServices.Collector.Collectors;

public class SystemCollector(
	Source source,
	ILogger<SystemCollector> logger) : CollectorBase(source, logger)
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
