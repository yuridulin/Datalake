using Datalake.PublicApi.Models.Sources;
using Datalake.Server.Services.Collection.Abstractions;
using Datalake.Server.Services.Maintenance;
using Datalake.Server.Services.Receiver;

namespace Datalake.Server.Services.Collection.Collectors;

internal class DatalakeCollector(
	ReceiverService receiverService,
	SourceWithTagsInfo source,
	SourcesStateService sourcesStateService,
	ILogger<DatalakeCollector> logger) : CollectorBase(source.Name, source, sourcesStateService, logger)
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

	protected override Task Work()
	{
		return Task.CompletedTask;
	}


	private readonly string? _address = source.Address;
}
