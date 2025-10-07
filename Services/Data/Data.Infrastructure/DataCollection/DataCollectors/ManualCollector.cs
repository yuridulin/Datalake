using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.DataCollectors;

[Transient]
public class ManualCollector(
	SourceSettingsDto source,
	ILogger<ManualCollector> logger) : DataCollectorBase(source, logger)
{
	protected override async Task Work()
	{
		await WriteAsync([]);
	}
}

