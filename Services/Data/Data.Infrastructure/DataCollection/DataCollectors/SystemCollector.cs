using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.DataCollectors;

[Transient]
public class SystemCollector(
	IDataCollectorWriter writer,
	ILogger<DatalakeCollector> logger,
	SourceSettingsDto source) : DataCollectorBase(writer, logger, source)
{
}
