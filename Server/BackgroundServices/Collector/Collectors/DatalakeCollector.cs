using DatalakeDatabase.Models;
using DatalakeServer.BackgroundServices.Collector.Collectors.Abstractions;
using DatalakeServer.Services.Receiver;

namespace DatalakeServer.BackgroundServices.Collector.Collectors;

/// <summary>
/// Сборщик данных из ноды Datalake
/// </summary>
/// <param name="receiverService">Служба получения данных</param>
/// <param name="source">Источник</param>
/// <param name="logger">Служба сообщений</param>
internal class DatalakeCollector(
	ReceiverService receiverService,
	Source source,
	ILogger<DatalakeCollector> logger) : CollectorBase(source, logger)
{
	public override event CollectEvent? CollectValues;

	public override Task Start()
	{
		CollectValues?.Invoke(this, []);
		try
		{
			if (!string.IsNullOrEmpty(_address))
				receiverService.AskDatalake([], _address).Wait();
		}
		catch { }

		return base.Start();
	}

	public override Task Stop()
	{
		return base.Stop();
	}

	private readonly string? _address = source.Address;
}
