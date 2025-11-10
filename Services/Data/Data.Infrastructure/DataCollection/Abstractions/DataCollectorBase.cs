using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.Abstractions;

/// <summary>
/// Базовый класс сборщика с реализацией основных механизмов
/// </summary>
public abstract class DataCollectorBase(
	IDataCollectorWriter writer,
	ILogger logger,
	SourceSettingsDto source,
	int workInterval = 1000) : IDataCollector, IAsyncDisposable
{
	protected readonly IDataCollectorWriter writer = writer;
	protected readonly ILogger logger = logger;
	protected readonly SourceSettingsDto source = source;

	public string Name { get; } = Source.InternalSources.Contains(source.SourceType)
		? source.SourceType.ToString()
		: $"{source.SourceName}<{source.SourceType}>#{source.SourceId}";

	public virtual Task StartAsync(CancellationToken cancellationToken)
	{
		logger.LogInformation("Запуск сборщика {name}", Name);
		_ = WorkAsync(cancellationToken);

		return Task.CompletedTask;
	}

	protected Task NotStartAsync(string reason)
	{
		logger.LogWarning("Сборщик {name} не будет запущен: {reason}", Name, reason);
		return Task.CompletedTask;
	}

	private async Task WorkAsync(CancellationToken cancellationToken)
	{
		DateTime executionStart;

		try
		{
			logger.LogInformation("Сборщик {name} начал работу", Name);

			while (!cancellationToken.IsCancellationRequested)
			{
				executionStart = DateTime.UtcNow;

				var newValues = await ExecuteAsync(cancellationToken);
				if (newValues.Count > 0)
				{
					await writer.WriteAsync(newValues);
				}

				var millisecondsToNextRun = workInterval - (int)(DateTime.UtcNow - executionStart).TotalMilliseconds;
				if (millisecondsToNextRun > 0)
				{
					await Task.Delay(millisecondsToNextRun, cancellationToken);
				}
			}
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogCritical(ex, "Критическая ошибка в рабочем цикле сборщика {name}", Name);
		}
		finally
		{
			logger.LogDebug("Сборщик {name} завершил работу", Name);
		}
	}

	protected virtual async Task<List<TagValue>> ExecuteAsync(CancellationToken cancellationToken) => [];

	public virtual async Task StopAsync() => await DisposeAsync();

	public async ValueTask DisposeAsync() => GC.SuppressFinalize(this);
}
