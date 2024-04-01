using DatalakeDatabase;

namespace DatalakeApp.Services
{
	public class CollectorWorker(ReceiverService receiverService, IServiceScopeFactory serviceScopeFactory) : BackgroundService
	{
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				using var scope = serviceScopeFactory.CreateScope();
				using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

				try
				{
					/*var dbLastUpdate = await db.Settings
						.Select(x => x.LastUpdate)
						.FirstAsync();*/

					/*if (Cache.LastUpdate > StoredUpdate)
					{
						Sources = db.Sources.ToList();
						Sources.ForEach(source => source.Rebuild(db));
						StoredUpdate = Cache.LastUpdate;
					}

					Sources.ForEach(source => source.Update(db));*/
				}
				catch (Exception ex)
				{
				}
				
				await Task.Delay(1000, stoppingToken);
			}
		}

		DateTime LastUpdate { get; set; } = DateTime.MinValue;
	}
}
