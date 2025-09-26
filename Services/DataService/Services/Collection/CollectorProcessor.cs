using Datalake.DataService.Abstractions;
using Datalake.PrivateApi.Attributes;
using Datalake.PublicApi.Models.Sources;
using LinqToDB;
using System.Collections.Concurrent;

namespace Datalake.DataService.Services.Collection;

/// <summary>
/// Менеджер сборщиков данных
/// </summary>
/// <param name="collectorFactory">Фабрика сборщиков</param>
/// <param name="collectorWriter">Сервис записи данных в БД</param>
/// <param name="logger">Логгер</param>
[Singleton]
public class CollectorProcessor(
	ICollectorFactory collectorFactory,
	ICollectorWriter collectorWriter,
	ILogger<CollectorProcessor> logger) : BackgroundService, ICollectorProcessor
{
	private CancellationToken _stoppingToken;
	private List<ICollector> _collectors = [];
	private readonly ConcurrentDictionary<ICollector, Task> _processingTasks = new();
	private readonly SemaphoreSlim _restartLock = new(1, 1);

	/// <inheritdoc/>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_stoppingToken = stoppingToken;
		await Task.Delay(Timeout.Infinite, stoppingToken);
	}

	/// <inheritdoc/>
	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		// Останавливаем все сборщики
		await StopCollecting();

		await base.StopAsync(cancellationToken);
	}

	public async Task UpdateAsync(IEnumerable<SourceWithTagsInfo> newSources)
	{
		if (!await _restartLock.WaitAsync(0, _stoppingToken))
			return;

		try
		{
			await RestartCollectors(newSources, _stoppingToken);
		}
		finally
		{
			_restartLock.Release();
		}
	}

	private async Task RestartCollectors(IEnumerable<SourceWithTagsInfo> newSources, CancellationToken stoppingToken)
	{
		logger.LogInformation("Выполняется обновление сборщиков");

		// Останавливаем текущие сборщики
		await StopCollecting();

		// Создаём новые сборщики
		_collectors = newSources
			.Where(x => !x.IsDisabled)
			.Select(collectorFactory.GetCollector)
			.Where(x => x != null)
			.Select(x => x!)
			.ToList();

		// Запускаем новые сборщики и обработчики их каналов
		foreach (var collector in _collectors)
		{
			collector.Start(stoppingToken);
			var processingTask = ProcessCollectorOutput(collector, stoppingToken);
			_processingTasks[collector] = processingTask;
		}

		logger.LogInformation("Обновление сборщиков завершено");
	}

	private async Task StopCollecting()
	{
		foreach (var collector in _collectors)
		{
			collector.PrepareToStop();
		}

		var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
		var completedTask = await Task.WhenAny(
			Task.WhenAll(_processingTasks.Values),
			timeoutTask
		);

		if (completedTask == timeoutTask)
		{
			logger.LogWarning("Таймаут остановки обработчиков!");
		}

		foreach (var collector in _collectors)
		{
			collector.FinalStop();
		}

		_processingTasks.Clear();
	}

	private async Task ProcessCollectorOutput(ICollector collector, CancellationToken stoppingToken)
	{
		try
		{
			await foreach (var batch in collector.OutputChannel.Reader.ReadAllAsync(stoppingToken))
			{
				if (batch?.Any() ?? false)
				{
					collectorWriter.AddToQueue(batch);
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