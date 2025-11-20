using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Interfaces.Storage;
using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.Collectors;

[Transient]
public class DatalakeCollector(
	ISourcesActivityStore sourcesActivityStore,
	IDataCollectorWriter writer,
	ILogger<DatalakeCollector> _,
	SourceSettingsDto source) : DataCollectorBase(sourcesActivityStore, writer, _, source)
{
	public override Task StartAsync(CancellationToken cancellationToken)
	{
		if (source.RemoteSettings == null)
			return NotStartAsync("нет настроек получения данных");

		if (string.IsNullOrEmpty(source.RemoteSettings.RemoteHost))
			return NotStartAsync("адрес для получения данных пуст");

		return base.StartAsync(cancellationToken);
	}
}
