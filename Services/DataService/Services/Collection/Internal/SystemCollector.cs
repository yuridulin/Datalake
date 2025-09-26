using Datalake.DataService.Services.Collection.Abstractions;
using Datalake.DataService.Services.Metrics;
using Datalake.PrivateApi.Attributes;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.DataService.Services.Collection.Internal;

[Transient]
public class SystemCollector(
	SourceWithTagsInfo source,
	SourcesStateService sourcesStateService,
	ILogger<SystemCollector> logger) : CollectorBase(source.Name, source, sourcesStateService, logger)
{
	protected override async Task Work()
	{
		await WriteAsync([]);
	}
}
