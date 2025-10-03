using Datalake.DataService.Services.Collection.Abstractions;
using Datalake.DataService.Services.Metrics;
using Datalake.Shared.Application;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.DataService.Services.Collection.External;

[Transient]
public class DatalakeCollector(
	SourceWithTagsInfo source,
	SourcesStateService sourcesStateService,
	ILogger<DatalakeCollector> logger) : CollectorBase(source.Name, source, sourcesStateService, logger)
{
	public override void Start(CancellationToken stoppingToken)
	{
		if (string.IsNullOrEmpty(_source.Address))
		{
			Task.Run(() => WriteAsync([], false), stoppingToken);
			_logger.LogWarning("Сборщик \"{name}\" не имеет адреса для получения данных и не будет запущен", _name);
			return;
		}

		Task.Run(() => WriteAsync([], true), stoppingToken);
		base.Start(stoppingToken);
	}

	protected override async Task Work()
	{
		await WriteAsync([]);
	}
}
