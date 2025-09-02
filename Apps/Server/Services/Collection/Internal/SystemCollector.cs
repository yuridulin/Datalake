using Datalake.PublicApi.Models.Sources;
using Datalake.Server.Services.Collection.Abstractions;
using Datalake.Server.Services.Maintenance;

namespace Datalake.Server.Services.Collection.Internal;

internal class SystemCollector(
	SourceWithTagsInfo source,
	SourcesStateService sourcesStateService,
	ILogger<SystemCollector> logger) : CollectorBase(source.Name, source, sourcesStateService, logger)
{
	protected override async Task Work()
	{
		await WriteAsync([]);
	}
}
