using Datalake.Database;
using Datalake.PublicApi.Constants;
using System.Diagnostics;

namespace Datalake.Server.BackgroundServices.HistoryIndexer;

/// <summary>
/// Индексирование таблиц истории после завершения активного периода
/// </summary>
public class HistoryIndexerService(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<HistoryIndexerService> logger) : BackgroundService
{
	DateTime LastIndexedTableDate = DateTime.MinValue.AddMinutes(1);

	/// <summary>
	/// Периодическая проверка необходимости создания индексов для таблиц истории позже сегодня
	/// </summary>
	/// <param name="stoppingToken">Токен остановки</param>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			bool hasError = false;
			try
			{
				using var scope = serviceScopeFactory.CreateScope();
				using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

				var tables = await db.TablesRepository.GetHistoryTablesFromSchema();

				var notIndexedTables = tables
					.Where(x => x.Date < DateFormats.GetCurrentDateTime().Date)
					.Where(x => x.Date > LastIndexedTableDate)
					.Where(x => !x.HasIndex)
					.OrderBy(x => x.Date)
					.ToArray();

				foreach (var table in notIndexedTables)
				{
					logger.LogInformation("Создание индекса для {name}", table.Name);

					try
					{
						var sw = Stopwatch.StartNew();
						await db.TablesRepository.CreateHistoryIndex(table.Name);

						LastIndexedTableDate = table.Date;

						sw.Stop();
						logger.LogDebug("Создание индекса для {name} завершено: {ms} мс", table.Name, sw.Elapsed.TotalMilliseconds);

						await Task.Delay(1000, stoppingToken);
					}
					catch (Exception ex)
					{
						logger.LogWarning("Не удалось создать индекс для {name}: {message}", table.Name, ex);
						hasError = true;
					}
				}
			}
			catch (Exception ex)
			{
				logger.LogWarning("Ошибка при индексации истории: {message}", ex);
				hasError = true;
			}

			await Task.Delay(hasError ? TimeSpan.FromMinutes(1) : TimeSpan.FromMinutes(5), stoppingToken);
		}
	}
}
