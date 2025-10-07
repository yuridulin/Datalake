using Datalake.Data.Application.Models.Sources;
using Datalake.Data.Infrastructure.DataCollection.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.DataCollectors;

[Transient]
public class DatalakeCollector(
	SourceSettingsDto source,
	ILogger<DatalakeCollector> logger) : DataCollectorBase(source, logger)
{
	public override void Start(CancellationToken stoppingToken)
	{
		if (_source.RemoteSettings == null)
		{
			Task.Run(() => WriteAsync([], false), stoppingToken);
			_logger.LogWarning("Сборщик \"{name}\" не имеет настроек получения данных и не будет запущен", _name);
			return;
		}

		if (string.IsNullOrEmpty(_source.RemoteSettings.RemoteHost))
		{
			Task.Run(() => WriteAsync([], false), stoppingToken);
			_logger.LogWarning("Сборщик \"{name}\" не имеет адреса для получения данных и не будет запущен", _name);
			return;
		}

		Task.Run(() => WriteAsync([], true), stoppingToken);
		base.Start(stoppingToken);
	}

	protected override async Task Work()
	{
		await WriteAsync([]);
	}
}
