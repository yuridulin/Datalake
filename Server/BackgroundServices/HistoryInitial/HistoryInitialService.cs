using Datalake.Database;
using System.Diagnostics;

namespace Datalake.Server.BackgroundServices.HistoryInitial;

/// <summary>
/// Проверка и воссоздание начальных (initial) значений в таблицах истории
/// </summary>
public class HistoryInitialService(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<HistoryInitialService> logger) : BackgroundService
{
	DateTime LastCheckedTableDate = DateTime.MinValue.AddMinutes(1);

	/// <summary>
	/// Периодическая проверка необходимости воссоздания начальных значений
	/// </summary>
	/// <param name="stoppingToken">Токен остановки</param>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			bool hasError = false;
			try
			{
				using var scope = serviceScopeFactory.CreateScope();
				using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

				var tables = await db.TablesRepository.GetHistoryTablesFromSchema();

				var notCheckedTables = tables
					.Where(x => x.Date > LastCheckedTableDate)
					.OrderBy(x => x.Date)
					.ToArray();

				foreach (var table in notCheckedTables)
				{
					logger.LogInformation("Проверка наличия начальных значений для {name}", table.Name);

					try
					{
						var sw = Stopwatch.StartNew();
						await db.TablesRepository.EnsureInitialValues(table.Date);

						LastCheckedTableDate = table.Date;

						sw.Stop();
						logger.LogDebug("Проверка наличия начальных значений для {name} завершена: {ms} мс",
							table.Name, sw.Elapsed.TotalMilliseconds);

						await Task.Delay(1000, stoppingToken);
					}
					catch (Exception ex)
					{
						logger.LogWarning("Не удалось проверить наличие начальных значений для {name}: {message}", table.Name, ex);
						hasError = true;
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning("Ошибка при проверке наличия начальных значений: {message}", ex);
				hasError = true;
			}

			await Task.Delay(hasError ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(5), stoppingToken);
		}
	}
}
