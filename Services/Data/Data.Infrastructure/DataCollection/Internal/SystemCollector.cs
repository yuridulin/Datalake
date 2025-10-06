using Datalake.Shared.Application;
using Datalake.PublicApi.Models.Sources;
using Datalake.Data.Host.Services.Metrics;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;

namespace Datalake.Data.Infrastructure.DataCollection.Internal;

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
