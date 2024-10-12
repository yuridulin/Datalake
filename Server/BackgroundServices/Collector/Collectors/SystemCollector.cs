using Datalake.ApiClasses.Models.Sources;
using Datalake.Server.BackgroundServices.Collector.Abstractions;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

/// <summary>
/// Системный сборщик данных, собирающий информацию о работе сервера
/// </summary>
/// <param name="source">Источник данных</param>
/// <param name="logger">Служба сообщений</param>
internal class SystemCollector(
	SourceWithTagsInfo source,
	ILogger<SystemCollector> logger) : CollectorBase(source, logger)
{
	public override event CollectEvent? CollectValues;

	public override Task Start(CancellationToken stoppingToken)
	{
		CollectValues?.Invoke(this, []);
		return base.Start(stoppingToken);
	}

	public override Task Stop()
	{
		return base.Stop();
	}
}
