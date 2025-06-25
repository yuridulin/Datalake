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
	ILogger<DatalakeCollector> logger) : CollectorBase(source.Name, source, logger)
{
	public override void Start(CancellationToken stoppingToken)
	{
		try
		{
			if (!string.IsNullOrEmpty(_address))
				receiverService.AskDatalake([], _address).Wait(stoppingToken);
		}
		catch { }

		base.Start(stoppingToken);
	}


	private readonly string? _address = source.Address;
}
