using DatalakeDatabase;
using DatalakeDatabase.Models;
using LinqToDB;

namespace DatalakeApp.Services
{
	public class CollectorWorker(
		ReceiverService receiverService,
		CacheService cacheService,
		IServiceScopeFactory serviceScopeFactory) : BackgroundService
	{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				using var scope = serviceScopeFactory.CreateScope();
				using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

				try
				{
					var dbLastUpdate = await db.Settings
						.Select(x => x.LastUpdate)
						.FirstAsync(token: stoppingToken);

					if (cacheService.LastUpdate > LastUpdate)
					{
						Sources = [.. db.Sources];
						//Sources.ForEach(source => source.Rebuild(db));
						LastUpdate = cacheService.LastUpdate;
					}

					foreach (var source in Sources)
					{
						await receiverService.GetItemsFromSourceAsync(source.Type, source.Address);
					}
					//Sources.ForEach(source => source.Update(db));
				}
				catch (Exception)
				{
				}
				
				await Task.Delay(1000, stoppingToken);
			}
		}

		DateTime LastUpdate { get; set; } = DateTime.MinValue;

		List<Source> Sources { get; set; } = [];
	}
}
