using Datalake.Database.Models;
using DatalakeServer.BackgroundServices.Collector.Collectors.Abstractions;

namespace DatalakeServer.BackgroundServices.Collector.Collectors;

/// <summary>
/// Системный сборщик данных, собирающий информацию о работе сервера
/// </summary>
/// <param name="source">Источник данных</param>
/// <param name="logger">Служба сообщений</param>
internal class SystemCollector(
	Source source,
	ILogger<SystemCollector> logger) : CollectorBase(source, logger)
{
	public override event CollectEvent? CollectValues;

	public override Task Start()
	{
		CollectValues?.Invoke(this, []);
		return base.Start();
	}

	public override Task Stop()
	{
		return base.Stop();
	}
}
