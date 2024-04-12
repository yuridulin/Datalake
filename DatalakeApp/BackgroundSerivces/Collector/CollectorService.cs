using DatalakeApp.BackgroundSerivces.Collector.Collectors.Abstractions;
using DatalakeApp.BackgroundSerivces.Collector.Collectors.Factory;
using DatalakeApp.BackgroundSerivces.Collector.Models;
using DatalakeDatabase;
using DatalakeDatabase.ApiModels.Values;
using DatalakeDatabase.Extensions;
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
					var sources = await db.Sources.ToListAsync(token: stoppingToken);
					Collectors.ForEach(x =>
					{
						x.Stop();
						x.CollectValues -= X_CollectValuesAsync;
					});
					Collectors = sources.Select(collectorFactory.GetCollector)
						.Where(x => x != null)
						.Select(x => x!)
						.ToList();
					Collectors.ForEach(x =>
					{
						x.CollectValues += X_CollectValuesAsync;
						x.Start();
					});
					LastUpdate = dbLastUpdate;
				}

				await Task.Delay(1000, stoppingToken);
			}
		}

		private void X_CollectValuesAsync(ICollector collector, IEnumerable<CollectValue> values)
		{
			using var scope = serviceScopeFactory.CreateScope();
			using var valuesRepository = scope.ServiceProvider.GetRequiredService<ValuesRepository>();

			
			Task
				.Run(async () =>
				{
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
					try
					{
						await valuesRepository.WriteValuesAsync(writeValues);
						logger.LogDebug("Collect values from {name} of {type} type: {count}", 
							collector.Name, collector.Type, writeValues.Length);
					}
					catch (Exception ex)
					{
						logger.LogError("Collect values from {name} of {type} throw: {message}",
							collector.Name, collector.Type, ex.Message);
					}
				})
				.Wait();
		}

		DateTime LastUpdate { get; set; } = DateTime.MinValue;

		List<ICollector> Collectors { get; set; } = [];
	}
}
