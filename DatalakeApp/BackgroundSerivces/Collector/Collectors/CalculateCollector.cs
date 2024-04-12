using DatalakeApp.BackgroundSerivces.Collector.Collectors.Abstractions;
using DatalakeDatabase.Models;

namespace DatalakeApp.BackgroundSerivces.Collector.Collectors;

public class CalculateCollector : CollectorBase
{
	public CalculateCollector(
		Source source,
		ILogger<CalculateCollector> logger) : base(source, logger)
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
