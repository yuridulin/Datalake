using DatalakeApp.BackgroundSerivces.Collector.Collectors.Abstractions;
using DatalakeApp.Services.Receiver;
using DatalakeDatabase.Models;

namespace DatalakeApp.BackgroundSerivces.Collector.Collectors;

public class DatalakeCollector : CollectorBase
{
	public DatalakeCollector(
		ReceiverService receiverService,
		Source source,
		ILogger<DatalakeCollector> logger) : base(source, logger)
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
