using Datalake.DataService.Services.Collection.Abstractions;
using Datalake.DataService.Services.Metrics;
using Datalake.Shared.Application;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.DataService.Services.Collection.Internal;

[Transient]
public class ManualCollector(
	SourceWithTagsInfo source,
	SourcesStateService sourcesStateService,
	ILogger<ManualCollector> logger) : CollectorBase(source.Name, source, sourcesStateService, logger)
{
	protected override async Task Work()
	{
		await WriteAsync([]);
	}
}

