using Datalake.PublicApi.Models.Sources;
using Datalake.Server.Services.Collection.Abstractions;
using Datalake.Server.Services.Maintenance;

namespace Datalake.Server.Services.Collection.Internal;

internal class ManualCollector(
	SourceWithTagsInfo source,
	SourcesStateService sourcesStateService,
	ILogger<ManualCollector> logger) : CollectorBase(source.Name, source, sourcesStateService, logger)
{
	protected override async Task Work()
	{
		await WriteAsync([]);
	}
}

