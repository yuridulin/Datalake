using DatalakeApp.BackgroundSerivces.Collector.Collectors.Abstractions;
using DatalakeDatabase.Models;

namespace DatalakeApp.BackgroundSerivces.Collector.Collectors;

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
		throw new NotImplementedException();
	}

	public override Task Stop()
	{
		throw new NotImplementedException();
	}
}
