using Datalake.ApiClasses.Models.Values;
using Datalake.Database;
using Datalake.Server.BackgroundServices.Collector.Models;

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
			CollectValue[] buffer;
			int allCount = Queue.Count;

			if (Queue.Count == 0)
			{
				await Task.Delay(250, stoppingToken);
				continue;
			}
			else if (Queue.Count > 1000)
			{
				buffer = Queue.Take(1000).ToArray();
				lock (Lock)
				{
					Queue = Queue.Skip(1000).ToList();
				}
			}
			else
			{
				buffer = [.. Queue];
				lock (Lock)
				{
					Queue = [];
				}
			}

			if (buffer.Length > 0)
			{
				try
				{
					logger.LogInformation("Запись значений из очереди: {length} из {all}", buffer.Length, allCount);
					await WriteValuesAsync(buffer);
				}
				catch (Exception ex)
				{
					logger.LogError("Ошибка при записи значений: {message}", ex.Message);

					lock (Lock)
					{
						Queue.AddRange(buffer);
					}
				}
			}

			await Task.Delay(250, stoppingToken);
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
	}
}
