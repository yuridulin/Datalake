using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Interfaces.Repositories;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.DataCollection;

[Singleton]
public class DataCollectorProcessor(
	ISourcesSettingsRepository sourcesSettingsRepository,
	IDataCollectorFactory dataCollectorFactory,
	IDataCollectorWriter dataWriter,
	ILogger<DataCollectorProcessor> logger) : IDataCollectorProcessor
{
	private List<IDataCollector> _collectors = [];
	private readonly ConcurrentDictionary<IDataCollector, Task> _activeCollectorsTasks = new();
	private readonly SemaphoreSlim _restartLock = new(1, 1);

	public async Task StartAsync(CancellationToken cancellationToken) => await UpdateAsync(cancellationToken);

	public async Task UpdateAsync(CancellationToken cancellationToken)
	{
		if (!await _restartLock.WaitAsync(0, cancellationToken))
			return;

		try
		{
			await RestartCollectors(cancellationToken);
		}
		finally
		{
			_restartLock.Release();
		}
	}

	private async Task RestartCollectors(CancellationToken cancellationToken)
	{
		logger.LogInformation("Выполняется обновление сборщиков");

		var sourcesSettings = await sourcesSettingsRepository.GetAllAsync(cancellationToken);

		// Останавливаем текущие сборщики
		await StopCollecting();

		// Создаём новые сборщики
		_collectors = sourcesSettings
			.Select(dataCollectorFactory.Create)
			.Where(x => x != null)
			.ToList()!;

		// Запускаем новые сборщики и обработчики их каналов
		foreach (var collector in _collectors)
		{
			collector.Start(cancellationToken);
			var processingTask = ProcessCollectorOutput(collector, cancellationToken);
			_activeCollectorsTasks[collector] = processingTask;

		}

		logger.LogInformation("Обновление сборщиков завершено");
	}

	private async Task StopCollecting()
	{
		foreach (var collector in _collectors)
			collector.PrepareToStop();

		var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
		var completedTask = await Task.WhenAny(
			Task.WhenAll(_activeCollectorsTasks.Values),
			timeoutTask
		);

		if (completedTask == timeoutTask)
		{
			logger.LogWarning("Таймаут остановки обработчиков!");
		}

		foreach (var collector in _collectors)
			collector.Stop();

		_activeCollectorsTasks.Clear();
	}

	private async Task ProcessCollectorOutput(IDataCollector collector, CancellationToken stoppingToken)
	{
		try
		{
			await foreach (var batch in collector.OutputChannel.Reader.ReadAllAsync(stoppingToken))
			{
				if (batch?.Any() ?? false)
				{
					dataWriter.AddToQueue(batch);
				}
			}
		}
		catch (OperationCanceledException)
		{
			// Корректное завершение при отмене
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка обработки вывода сборщика {CollectorName}", collector.Name);
		}
	}
}
