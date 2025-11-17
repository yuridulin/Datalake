using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.Collectors;

[Transient]
public class SystemCollector(
	ISourcesActivityStore sourcesActivityStore,
	IDataCollectorWriter writer,
	ILogger<DatalakeCollector> logger,
	SourceSettingsDto source) : DataCollectorBase(sourcesActivityStore, writer, logger, source)
{
}
