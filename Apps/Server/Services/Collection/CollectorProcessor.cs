using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Models;
using Datalake.Database.InMemory.Queries;
using Datalake.Server.Services.Collection.Abstractions;
using Datalake.Server.Services.Maintenance;
using LinqToDB;
using System.Collections.Concurrent;

namespace Datalake.Server.Services.Collection;

/// <summary>
/// Менеджер сборщиков данных
/// </summary>
/// <param name="collectorFactory">Фабрика сборщиков</param>
/// <param name="collectorWriter">Сервис записи данных в БД</param>
/// <param name="sourcesStateService">Сервис отслеживания активности источников данных</param>
/// <param name="dataStore">Хранилище данных приложения</param>
/// <param name="logger">Логгер</param>
public class CollectorProcessor(
	CollectorFactory collectorFactory,
	CollectorWriter collectorWriter,
	SourcesStateService sourcesStateService,
	DatalakeDataStore dataStore,
	ILogger<CollectorProcessor> logger) : BackgroundService
{
	private CancellationToken _stoppingToken;
	private List<ICollector> _collectors = [];
	private readonly ConcurrentDictionary<ICollector, Task> _processingTasks = new();
	private readonly SemaphoreSlim _restartLock = new(1, 1);

	/// <inheritdoc/>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		_stoppingToken = stoppingToken;

		await OnStateChanged(dataStore.State);

		dataStore.StateChanged += (_, state) => Task.Run(() => OnStateChanged(state), stoppingToken);

		await Task.Delay(Timeout.Infinite, stoppingToken);
	}

	/// <inheritdoc/>
	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		// Останавливаем все сборщики
		await StopCollecting();

		await base.StopAsync(cancellationToken);
	}

	private async Task OnStateChanged(DatalakeDataState state)
	{
		if (!await _restartLock.WaitAsync(0, _stoppingToken))
			return;

		try
		{
			await RestartCollectors(state, _stoppingToken);
		}
		finally
		{
			_restartLock.Release();
		}
	}

	private async Task RestartCollectors(DatalakeDataState state, CancellationToken stoppingToken)
	{
		logger.LogInformation("Выполняется обновление сборщиков");

		// Останавливаем текущие сборщики
		await StopCollecting();

		// Создаём новые сборщики
		var newSources = state.SourcesInfoWithTagsAndSourceTags().ToArray();
		_collectors = newSources
			.Where(x => !x.IsDisabled)
			.Select(collectorFactory.GetCollector)
			.Where(x => x != null)
			.Select(x => x!)
			.ToList();

		sourcesStateService.Initialize(newSources.Select(x => x.Id).ToArray());

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