using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.DataCollectors;

[Transient]
public class DatalakeCollector(
	IDataCollectorProcessor processor,
	ILogger<DatalakeCollector> logger,
	SourceSettingsDto source) : DataCollectorBase(processor, logger, source)
{
	public override Task StartAsync(CancellationToken stoppingToken)
	{
		if (source.RemoteSettings == null)
		{
			this.logger.LogWarning("Сборщик {name} не имеет настроек получения данных и не будет запущен", Name);
			return Task.CompletedTask;
		}

		if (string.IsNullOrEmpty(source.RemoteSettings.RemoteHost))
		{
			this.logger.LogWarning("Сборщик \"{name}\" не имеет адреса для получения данных и не будет запущен", Name);
			return Task.CompletedTask;
		}

		return base.StartAsync(stoppingToken);
	}

	protected override Task WorkAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
