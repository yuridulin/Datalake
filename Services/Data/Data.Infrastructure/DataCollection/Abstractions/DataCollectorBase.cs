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
		double elapsed;
		int delay;
		CollectorUpdate state = new()
		{
			Values = new(source.Tags.Count()),
			IsActive = false,
		};

		try
		{
			logger.LogInformation("Сборщик {name} начал работу", Name);

			while (!cancellationToken.IsCancellationRequested)
			{
				executionStart = DateTime.UtcNow;

				state.Values.Clear();
				await ExecuteAsync(state, cancellationToken);
				for (int i = 0; i < state.Values.Count; i++)
				{
					await writer.WriteAsync(state.Values[i]);
				}
				// TODO: запись количества изменений и состояние источника в стор статистики

				elapsed = (DateTime.UtcNow - executionStart).TotalMilliseconds;
				delay = Math.Max(500, workInterval - (int)elapsed);

				if (state.Values.Count > 0)
					logger.LogDebug("Сборщик {name} собрал значения: {count} за {elapsed} мс", Name, state.Values.Count, elapsed);

				await Task.Delay(delay, cancellationToken);
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

	protected virtual Task ExecuteAsync(CollectorUpdate state, CancellationToken cancellationToken) => Task.CompletedTask;

	public virtual async Task StopAsync() => await DisposeAsync();

	public async ValueTask DisposeAsync() => GC.SuppressFinalize(this);

	protected class CollectorUpdate
	{
		public required List<TagValue> Values { get; set; }

		public bool IsActive { get; set; }
	}
}
