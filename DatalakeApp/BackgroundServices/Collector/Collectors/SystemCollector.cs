using DatalakeApp.BackgroundServices.Collector.Collectors.Abstractions;
using DatalakeDatabase.Models;

namespace DatalakeApp.BackgroundServices.Collector.Collectors;

public class SystemCollector : CollectorBase
{
	public SystemCollector(
		Source source,
		ILogger<SystemCollector> logger) : base(source, logger)
	{
	}

	public override event CollectEvent? CollectValues;

	public override Task Start()
	{
		return base.Start();
	}

	public override Task Stop()
	{
		return base.Stop();
	}
}
