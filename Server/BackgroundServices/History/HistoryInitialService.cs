using Datalake.Database;
using Datalake.Database.Repositories;

namespace Datalake.Server.BackgroundServices.History;

/// <summary>
/// Проверка наличия Initial значений в партициях и создание по необходимости
/// </summary>
public class HistoryInitialService(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<HistoryIndexerService> logger) : BackgroundService
{
	Dictionary<DateTime, bool> TablesWithInitialValues { get; set; } = [];

	/// <inheritdoc />
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				var tables = TablesRepository.CachedTables.Keys.OrderByDescending(x => x).ToArray();

				foreach (var date in tables)
				{
					bool hasInitial = TablesWithInitialValues.TryGetValue(date, out bool b) && b;
					if (hasInitial)
					{
						continue;
					}
					else
					{
						using var scope = serviceScopeFactory.CreateScope();
						using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

						try
						{
							await db.TablesRepository.EnsureInitialValues(date);

							TablesWithInitialValues[date] = true;
							logger.LogInformation("Initial значения созданы для {date}", date.ToString("yyyy-MM-dd"));

							await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
						}
						catch
						{
							TablesWithInitialValues[date] = false;
						}
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "проверка наличия initial значений");
			}
			finally
			{
				await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
			}
		}
	}
}
