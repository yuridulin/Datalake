using Datalake.Database;
using Datalake.Database.Models;
using Datalake.Database.Utilities;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.BackgroundServices.Collector.Models;
using LinqToDB;
using System.Diagnostics;

namespace Datalake.Server.BackgroundServices.Collector;

internal class CollectorProcessor(
	CollectorFactory collectorFactory,
	IServiceScopeFactory serviceScopeFactory,
	ILogger<CollectorProcessor> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var lastUpdate = Cache.LastUpdate;

			if (lastUpdate > StoredUpdate)
			{
				var sw = Stopwatch.StartNew();
				logger.LogInformation("Обновление сборщиков");

				List<Source> newSources = [];

				try
				{
					using var scope = serviceScopeFactory.CreateScope();
					using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

					var query =
						from source in db.Sources
						from tag in db.Tags.Where(x => !string.IsNullOrEmpty(x.SourceItem)).LeftJoin(x => x.SourceId == source.Id)
						group new { source, tag } by source into g
						select new Source
						{
							Id = g.Key.Id,
							Address = g.Key.Address,
							Name = g.Key.Name,
							Type = g.Key.Type,
							Tags = g.Select(x => x.tag).Where(x => x != null).ToArray(),
						};

					newSources = await query.ToListAsync(token: stoppingToken);
				}
				catch (Exception ex)
				{
					logger.LogError("Ошибка при получении информации о источниках: {message}", ex.Message);
				}

				if (newSources.Count != 0)
				{
					Collectors.ForEach(x =>
					{
						x.Stop();
						x.CollectValues -= CollectValues;
					});

					Collectors = newSources.Select(collectorFactory.GetCollector)
						.Where(x => x != null)
						.Select(x => x!)
						.ToList();

					Collectors.ForEach(x =>
					{
						x.CollectValues += CollectValues;
						x.Start(stoppingToken);
					});

					StoredUpdate = lastUpdate;
				}

				sw.Stop();
				logger.LogInformation("Обновление сборщиков завершено: [{n}] за {ms} мс", Collectors.Count, sw.Elapsed.TotalMilliseconds);
			}

			await Task.Delay(5000, stoppingToken);
		}
	}

	private void CollectValues(ICollector collector, IEnumerable<CollectValue> values)
	{
		lock (CollectorWriter.Lock)
		{
			CollectorWriter.Queue.AddRange(values);
		}
	}

	private DateTime StoredUpdate { get; set; } = DateTime.MinValue;

	private List<ICollector> Collectors { get; set; } = [];
}
