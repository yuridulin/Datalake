using DatalakeApp.Services.Collector.Collectors.Abstractions;
using DatalakeApp.Services.Collector.Collectors.Factory;
using DatalakeApp.Services.Collector.Models;
using DatalakeDatabase;
using DatalakeDatabase.ApiModels.Values;
using DatalakeDatabase.Repositories;
using LinqToDB;

namespace DatalakeApp.Services.Collector
{
	public class CollectorService(
		CollectorFactory collectorFactory,
		IServiceScopeFactory serviceScopeFactory) : BackgroundService
	{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				using var scope = serviceScopeFactory.CreateScope();
				using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

				var dbLastUpdate = await db.Settings
					.Select(x => x.LastUpdate)
					.FirstAsync(token: stoppingToken);

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

		private void X_CollectValuesAsync(IEnumerable<CollectValue> values)
		{
			using var scope = serviceScopeFactory.CreateScope();
			using var valuesRepository = scope.ServiceProvider.GetRequiredService<ValuesRepository>();

			Task
				.Run(async () =>
				{
					try
					{
						await valuesRepository.WriteValuesAsync(values
							.Select(x => new ValueWriteRequest
							{
								TagId = x.TagId,
								TagName = null,
								Date = x.DateTime,
								Value = x.Value,
								TagQuality = x.Quality,
							})
							.ToArray());
					}
					catch (Exception)
					{

					}
				})
				.Wait();
		}

		DateTime LastUpdate { get; set; } = DateTime.MinValue;

		List<ICollector> Collectors { get; set; } = [];
	}
}
