using Datalake.Database;
using Datalake.Database.Repositories;
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
		using var scope = serviceScopeFactory.CreateScope();
		using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();

		while (!stoppingToken.IsCancellationRequested)
		{
			CollectValue[] buffer;
			int allCount = Queue.Count;

			if (Queue.Count == 0)
			{
				await Task.Delay(AfterWriteDelay, stoppingToken);
				continue;
			}
			else if (Queue.Count > BufferSize)
			{
				buffer = Queue.Take(BufferSize).ToArray();
				lock (Lock)
				{
					Queue = Queue.Skip(BufferSize).ToList();
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
					var ms = await ValuesRepository.WriteValuesAsSystemAsync(db, buffer);

					logger.LogInformation("Запись значений из очереди: {length} из {all} за {ms}мс", buffer.Length, allCount, ms);
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

			await Task.Delay(AfterWriteDelay, stoppingToken);
		}
	}

	/// <summary>
	/// Добавление новых значений в очередь на запись в БД
	/// </summary>
	/// <param name="values">Список новых значений</param>
	public static void AddToQueue(IEnumerable<CollectValue> values)
	{
		lock (Lock)
		{
			Queue.AddRange(values);
		}
	}

	/// <summary>
	/// Количество элементов, записываемых за один запрос
	/// </summary>
	private const int BufferSize = 1000;

	/// <summary>
	/// Ожидание после завершения записи до начала следующей записи
	/// </summary>
	private const int AfterWriteDelay = 250;

	/// <summary>
	/// Очередь новых значений на запись в БД
	/// </summary>
	private static List<CollectValue> Queue { get; set; } = [];

	/// <summary>
	/// Объект блокировки операций с очередью
	/// </summary>
	private static readonly object Lock = new();
}
