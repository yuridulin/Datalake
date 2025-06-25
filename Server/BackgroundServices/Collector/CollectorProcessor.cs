using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Models;
using Datalake.Database.InMemory.Queries;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.BackgroundServices.Collector.Models;
using Datalake.Server.Services.StateManager;
using LinqToDB;

namespace Datalake.Server.BackgroundServices.Collector;

internal class CollectorProcessor(
	CollectorFactory collectorFactory,
	SourcesStateService sourcesStateService,
	DatalakeDataStore dataStore,
	ILogger<CollectorProcessor> logger) : BackgroundService
{

	private List<ICollector> _collectors = [];

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		dataStore.StateChanged += (_, state) => Task.Run(() => RestartCollectors(state, stoppingToken));

		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(1000, stoppingToken);
		}
	}

	private void RestartCollectors(DatalakeDataState state, CancellationToken stoppingToken)
	{
		logger.LogInformation("Выполняется обновление сборщиков");

		var newSources = state.SourcesInfoWithTagsAndSourceTags().ToArray();

		_collectors.ForEach(x =>
		{
			x.Stop();
			x.CollectValues -= CollectValues;
		});

		_collectors = newSources.Select(collectorFactory.GetCollector)
			.Where(x => x != null)
			.Select(x => x!)
			.ToList();

		sourcesStateService.Initialize(newSources.Select(x => x.Id).ToArray());

		_collectors.ForEach(x =>
		{
			x.CollectValues += CollectValues;
			x.Start(stoppingToken);
		});

		logger.LogInformation("Обновление сборщиков завершено");
	}

	private void CollectValues(ICollector collector, IEnumerable<CollectValue> values)
	{
		CollectorWriter.AddToQueue(values);
	}
}
