using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Exceptions;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection.Abstractions;

/// <summary>
/// Базовый класс сборщика с реализацией основных механизмов
/// </summary>
public abstract class DataCollectorBase(
	IDataCollectorProcessor processor,
	ILogger logger,
	SourceSettingsDto source,
	int workInterval = 1000) : IDataCollector
{
	protected readonly IDataCollectorProcessor processor = processor;
	protected readonly ILogger logger = logger;
	protected readonly SourceSettingsDto source = source;
	protected volatile bool isRunning = false;

	private readonly CancellationTokenSource localCts = new();
	private Task workLoopTask = Task.CompletedTask;

	public string Name { get; } = Source.InternalSources.Contains(source.SourceType)
		? source.SourceType.ToString()
		: $"{source.SourceName}<{source.SourceType}>#{source.SourceId}";

	public virtual Task StartAsync(CancellationToken cancellationToken = default)
	{
		if (isRunning)
			return Task.CompletedTask;

		isRunning = true;
		logger.LogInformation("Запуск сборщика {name}", Name);
		try
		{
			// создаем локальный токен отмены, который также сработает, если внешний процессор пришлет отмену
			var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, localCts.Token);

			workLoopTask = WorkLoopAsync(linkedCts.Token);
			return Task.CompletedTask;
		}
		catch (Exception ex)
		{
			isRunning = false;
			logger.LogError(ex, "Критическая ошибка при запуске сборщика {name}", Name);

			throw new InfrastructureException($"Не удалось запустить сборщик {Name}: {ex.Message}");
		}
	}

	public virtual async Task StopAsync()
	{
		if (!isRunning)
			return;

		logger.LogDebug("Остановка сборщика {name}", Name);

		isRunning = false;
		localCts.Cancel();

		try
		{
			var timeoutTask = Task.Delay(TimeSpan.FromSeconds(5));
			var completedTask = await Task.WhenAny(workLoopTask, timeoutTask);

			if (completedTask == timeoutTask)
			{
				logger.LogWarning("Таймаут остановки сборщика {name}", Name);
			}
		}
		catch (Exception ex)
		{
			logger.LogWarning(ex, "Ошибка при остановке сборщика {name}", Name);
		}

		logger.LogInformation("Сборщик {name} остановлен", Name);
	}

	protected abstract Task WorkAsync(CancellationToken cancellationToken);

	protected async Task WriteValuesAsync(IReadOnlyCollection<TagValue> values, CancellationToken cancellationToken)
	{
		await processor.WriteValuesAsync(values, cancellationToken);
	}

	private async Task WorkLoopAsync(CancellationToken cancellationToken)
	{
		try
		{
			logger.LogInformation("Сборщик {name} начал работу", Name);

			while (isRunning && !cancellationToken.IsCancellationRequested)
			{
				try
				{
					await WorkAsync(cancellationToken);

					if (workInterval > 0)
					{
						await Task.Delay(workInterval, cancellationToken);
					}
				}
				catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
				{
					logger.LogDebug("Работа сборщика {name} отменена", Name);
					break;
				}
				catch (Exception ex)
				{
					logger.LogError(ex, "Ошибка в работе сборщика {name}", Name);

					// пауза перед повторной попыткой
					if (isRunning)
					{
						await Task.Delay(5000, cancellationToken);
					}
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
}
