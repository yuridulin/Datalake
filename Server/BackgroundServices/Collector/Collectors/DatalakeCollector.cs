using Datalake.PublicApi.Models.Sources;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.Services.Receiver;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

/// <summary>
/// Сборщик данных из ноды Datalake
/// </summary>
/// <param name="receiverService">Служба получения данных</param>
/// <param name="source">Источник</param>
/// <param name="logger">Служба сообщений</param>
internal class DatalakeCollector(
	ReceiverService receiverService,
	SourceWithTagsInfo source,
	ILogger<DatalakeCollector> logger) : CollectorBase(source, logger)
{
	public override event CollectEvent? CollectValues;

	public override Task Start(CancellationToken stoppingToken)
	{
		CollectValues?.Invoke(this, []);
		try
		{
			if (!string.IsNullOrEmpty(_address))
				receiverService.AskDatalake([], _address).Wait(stoppingToken);
		}
		catch { }

		return base.Start(stoppingToken);
	}

	public override Task Stop()
	{
		return base.Stop();
	}

	private readonly string? _address = source.Address;
}
