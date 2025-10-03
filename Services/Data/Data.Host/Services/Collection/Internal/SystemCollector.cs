using Datalake.Shared.Application;
using Datalake.PublicApi.Models.Sources;
using Datalake.Data.Host.Services.Metrics;
using Datalake.Data.Host.Services.Collection.Abstractions;

namespace Datalake.Data.Host.Services.Collection.Internal;

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
