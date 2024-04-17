using DatalakeApp.BackgroundSerivces.Collector.Collectors.Abstractions;
using DatalakeApp.BackgroundSerivces.Collector.Collectors.Factory;
using DatalakeApp.BackgroundSerivces.Collector.Models;
using DatalakeDatabase;
using DatalakeDatabase.ApiModels.Values;
using DatalakeDatabase.Extensions;
using DatalakeDatabase.Models;
using DatalakeDatabase.Repositories;
using LinqToDB;

namespace DatalakeApp.BackgroundSerivces.Collector
{
	public class CollectorService(
		CollectorFactory collectorFactory,
		IServiceScopeFactory serviceScopeFactory,
		ILogger<CollectorService> logger) : BackgroundService
	{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				using var scope = serviceScopeFactory.CreateScope();
				using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

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
						x.Start();
					});
					LastUpdate = dbLastUpdate;

					logger.LogWarning("Rebuild sources completed");
				}

				await Task.Delay(5000, stoppingToken);
			}
		}

		private void X_CollectValuesAsync(ICollector collector, IEnumerable<CollectValue> values)
		{
			using var scope = serviceScopeFactory.CreateScope();
			using var valuesRepository = scope.ServiceProvider.GetRequiredService<ValuesRepository>();

			var writeValues = values
				.Select(x => new ValueWriteRequest
				{
					TagId = x.TagId,
					TagName = null,
					Date = x.DateTime,
					Value = x.Value,
					TagQuality = x.Quality,
				})
				.ToArray();

			valuesRepository.WriteValuesAsync(writeValues).Wait();
			logger.LogDebug("Collect from {name} of {type} type: {count} values", 
				collector.Name, collector.Type, writeValues.Length);
		}

		DateTime LastUpdate { get; set; } = DateTime.MinValue;

		List<ICollector> Collectors { get; set; } = [];
	}
}
