using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Datalake.Data.Infrastructure.DataCollection;

[Singleton]
public class DataCollectorProcessor(
	IDataCollectorFactory dataCollectorFactory,
	IDataCollectorWriter dataWriter,
	ILogger<DataCollectorProcessor> logger) : IDataCollectorProcessor
{
	private List<IDataCollector> _collectors = [];
	private readonly ConcurrentDictionary<IDataCollector, Task> _activeCollectorsTasks = new();
	private readonly SemaphoreSlim _restartLock = new(1, 1);

	public async Task RestartAsync(IEnumerable<SourceSettingsDto> sourcesSettings)
	{
		await _restartLock.WaitAsync();

		try
		{
			await RestartCollectors(sourcesSettings);
		}
		finally
		{
			_restartLock.Release();
		}
	}

	private async Task RestartCollectors(IEnumerable<SourceSettingsDto> sourcesSettings)
	{
		logger.LogInformation("Выполняется обновление сборщиков");

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
			collector.Start();
			var processingTask = ProcessCollectorOutput(collector);
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

	private async Task ProcessCollectorOutput(IDataCollector collector)
	{
		try
		{
			await foreach (var batch in collector.OutputChannel.Reader.ReadAllAsync())
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
