using Datalake.Database;
using Datalake.Database.Repositories;
using System.Diagnostics;

namespace Datalake.Server.BackgroundServices.HistoryInitial;

/// <summary>
/// Проверка и воссоздание начальных (initial) значений в таблицах истории
/// </summary>
public class HistoryInitialService(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<HistoryInitialService> logger) : BackgroundService
{
	/// <summary>
	/// Периодическая проверка необходимости воссоздания начальных значений
	/// </summary>
	/// <param name="stoppingToken">Токен остановки</param>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			bool hasError = false;

			try
			{
				using var scope = serviceScopeFactory.CreateScope();
				using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

				var tables = await TablesRepository.GetHistoryTablesFromSchema(db);
				tables = [.. tables.Skip(1).OrderBy(x => x.Date)];

				logger.LogInformation("Запущена проверка наличия начальных значений");

				foreach (var table in tables)
				{
					logger.LogInformation("Проверка наличия начальных значений для {name}", table.Name);

					var sw = Stopwatch.StartNew();
					await TablesRepository.EnsureInitialValues(db, table.Date);

					sw.Stop();
					logger.LogDebug("Проверка наличия начальных значений для {name} завершена: {ms} мс",
						table.Name, sw.Elapsed.TotalMilliseconds);

					await Task.Delay(1000, stoppingToken);
				}

				logger.LogInformation("Проверка наличия начальных значений завершена");
			}
			catch (Exception ex)
			{
				logger.LogWarning("Ошибка при проверке наличия начальных значений: {message}", ex);
				hasError = true;
			}

			await Task.Delay(hasError ? TimeSpan.FromMinutes(5) : TimeSpan.FromMinutes(30), stoppingToken);
		}
	}
}
