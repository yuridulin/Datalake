using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.Server.BackgroundServices.Collector.Abstractions;

/// <summary>
/// Базовый класс сборщика с реализацией основных механизмов
/// </summary>
/// <param name="name">Название источника данных</param>
/// <param name="source">Данные источника данных, необходимые для запуска сбора</param>
/// <param name="logger">Служба сообщений</param>
internal abstract class CollectorBase(string name, SourceWithTagsInfo source, ILogger logger) : ICollector
{
	protected readonly string _name = name;
	protected readonly ILogger _logger = logger;
	protected readonly SourceType _sourceType = source.Type;

	public virtual Task Start(CancellationToken stoppingToken)
	{
		_logger.LogDebug("Сборщик {name} запущен", _name);
		return Task.CompletedTask;
	}

	public virtual Task Stop()
	{
		_logger.LogDebug("Сборщик {name} остановлен", _name);
		return Task.CompletedTask;
	}

	public abstract event CollectEvent CollectValues;
}
