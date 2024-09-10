using Datalake.ApiClasses.Models.Values;
using Datalake.Database;
using Datalake.Database.Extensions;
using Datalake.Database.Models;
using Datalake.Database.Repositories;
using Datalake.Server.BackgroundServices.Collector.Abstractions;
using Datalake.Server.BackgroundServices.Collector.Models;
using LinqToDB;
using System.Diagnostics;

namespace Datalake.Server.BackgroundServices.Collector;

internal class CollectorService(
	CollectorFactory collectorFactory,
	IServiceScopeFactory serviceScopeFactory,
	ILogger<CollectorService> logger) : BackgroundService
{
	static SemaphoreSlim semaphore = new(1,1);

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = serviceScopeFactory.CreateScope();
		using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		while (!stoppingToken.IsCancellationRequested)
		{
			var dbLastUpdate = await db.GetLastUpdateAsync();

			if (dbLastUpdate > LastUpdate)
			{
				logger.LogWarning("Rebuild sources");

				var query = from source in db.Sources
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

				var sourcesWithTags = await query.ToListAsync(token: stoppingToken);

				Collectors.ForEach(x =>
				{
					x.Stop();
					x.CollectValues -= X_CollectValuesAsync;
				});
				Collectors = sourcesWithTags.Select(collectorFactory.GetCollector)
					.Where(x => x != null)
					.Select(x => x!)
					.ToList();
				Collectors.ForEach(x =>
				{
					x.CollectValues += X_CollectValuesAsync;
					x.Start(stoppingToken);
				});

				LastUpdate = dbLastUpdate;

				logger.LogWarning("Rebuild sources completed");
			}

			await Task.Delay(5000, stoppingToken);
		}
	}

	private async void X_CollectValuesAsync(ICollector collector, IEnumerable<CollectValue> values)
	{
		int count = values.Count();
		if (count > 0)
		{
			var watch = Stopwatch.StartNew();
			await semaphore.WaitAsync();

			try
			{
				using var scope = serviceScopeFactory.CreateScope();
				using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();
				using var repository = new ValuesRepository(db);

				var writeValues = values
					.Select(x => new ValueWriteRequest
					{
						Guid = x.Guid,
						Date = x.DateTime,
						Value = x.Value,
						Quality = x.Quality,
					})
					.ToArray();

				await repository.WriteValuesAsync(writeValues);

				logger.LogDebug("Write tags to db: {count}", values.Count());

			}
			finally
			{
				watch.Stop();
				Debug.WriteLine($"Запись значений за {watch.Elapsed.TotalMilliseconds}");
				semaphore.Release();
			}
		}
	}

	DateTime LastUpdate { get; set; } = DateTime.MinValue;

	List<ICollector> Collectors { get; set; } = [];
}
