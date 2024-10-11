using Datalake.ApiClasses.Models.Values;
using Datalake.Database;
using Datalake.Server.BackgroundServices.Collector.Models;
using System.Diagnostics;

namespace Datalake.Server.BackgroundServices.Collector;

/// <summary>
/// Служба записи новых значений в БД
/// </summary>
/// <param name="serviceScopeFactory">Провайдер получения сервисов</param>
/// <param name="logger">Логгер</param>
public class CollectorWriter(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<CollectorWriter> logger) : BackgroundService
{
	/// <summary>
	/// Цикличная запись значений из очереди в БД
	/// </summary>
	/// <param name="stoppingToken">Токен остановки</param>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var buffer = Queue.Take(1000).ToArray();

			if (buffer.Length > 0)
			{
				try
				{
					logger.LogInformation("Запись значений из очереди: {} шт.", buffer.Length);
					await WriteValuesAsync(buffer);

					lock (Lock)
					{
						Queue = Queue.Except(buffer).ToList();
					}
				}
				catch (Exception ex)
				{
					logger.LogError("Ошибка при записи значений: {message}", ex.Message);
				}
			}

			await Task.Delay(1000, stoppingToken);
		}
	}

	/// <summary>
	/// Очередь новых значений на запись в БД
	/// </summary>
	public static List<CollectValue> Queue { get; set; } = [];

	/// <summary>
	/// Объект блокировки операций с очередью
	/// </summary>
	public static readonly object Lock = new();

	private async Task WriteValuesAsync(IEnumerable<CollectValue> values)
	{
		logger.LogInformation("Событие записи значений");
		var sw = Stopwatch.StartNew();

		using var scope = serviceScopeFactory.CreateScope();
		using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		var writeValues = values
			.Select(x => new ValueWriteRequest
			{
				Guid = x.Guid,
				Date = x.DateTime,
				Value = x.Value,
				Quality = x.Quality,
			})
			.ToArray();

		await db.ValuesRepository.WriteValuesAsSystemAsync(writeValues);

		sw.Stop();
		logger.LogInformation("Событие записи значений: {ms} мс", sw.Elapsed.TotalMilliseconds);
	}
}
