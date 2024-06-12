using Datalake.Database.Models;
using Datalake.Server.BackgroundServices.Collector.Collectors.Abstractions;

namespace Datalake.Server.BackgroundServices.Collector.Collectors;

/// <summary>
/// Источник данных, выполняющий расчёты по формулам
/// </summary>
/// <param name="source">Источник</param>
/// <param name="logger">Служба сообщений</param>
internal class CalculateCollector(
	Source source,
	ILogger<CalculateCollector> logger) : CollectorBase(source, logger)
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
