using Datalake.Database;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Models.Values;
using System.Threading.Channels;

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
	private const int BatchSize = 1000;
	private const int MaxBatchDelayMs = 250;
	private const int MinBatchDelayMs = 50;
	private const int MaxRetryAttempts = 3;
	private const int RetryBaseDelayMs = 1000;

	private readonly Channel<ValueWriteRequest> _channel = Channel.CreateUnbounded<ValueWriteRequest>(
		new UnboundedChannelOptions
		{
			SingleWriter = false,
			SingleReader = true,
			AllowSynchronousContinuations = false
		});

	/// <summary>
	/// Цикличная запись значений из очереди в БД
	/// </summary>
	/// <param name="stoppingToken">Токен остановки</param>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				// Создаем новый скоуп для каждого цикла обработки
				using var scope = serviceScopeFactory.CreateScope();
				using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();
				var valuesRepository = scope.ServiceProvider.GetRequiredService<ValuesRepository>();

				// Ждем первого элемента с таймаутом
				var batch = new List<ValueWriteRequest>(BatchSize);
				var firstItem = await ReadWithTimeoutAsync(stoppingToken);
				if (firstItem != null)
				{
					batch.Add(firstItem);
				}
				else
				{
					// Если данных нет, делаем паузу перед следующей проверкой
					await Task.Delay(MaxBatchDelayMs, stoppingToken);
					continue;
				}

				// Быстро собираем остальные элементы
				await FillBatchAsync(batch, BatchSize - 1, stoppingToken);

				// Записываем пачку в БД
				await WriteBatchWithRetryAsync(db, valuesRepository, batch, stoppingToken);
			}
			catch (OperationCanceledException)
			{
				// Корректное завершение при отмене
				break;
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Критическая ошибка в обработчике записи");
				await Task.Delay(5000, stoppingToken); // Задержка перед повторной попыткой
			}
		}
	}

	/// <summary>
	/// Добавление новых значений в очередь на запись в БД
	/// </summary>
	/// <param name="values">Список новых значений</param>
	public void AddToQueue(IEnumerable<ValueWriteRequest> values)
	{
		foreach (var value in values)
		{
			_channel.Writer.TryWrite(value);
		}
	}

	private async Task<ValueWriteRequest?> ReadWithTimeoutAsync(CancellationToken ct)
	{
		try
		{
			var readTask = _channel.Reader.ReadAsync(ct).AsTask();
			var timeoutTask = Task.Delay(MaxBatchDelayMs, ct);

			var completedTask = await Task.WhenAny(readTask, timeoutTask);
			if (completedTask == readTask)
			{
				return await readTask;
			}
		}
		catch (OperationCanceledException) { }
		return null;
	}

	private async Task FillBatchAsync(List<ValueWriteRequest> batch, int maxItems, CancellationToken ct)
	{
		var delayTask = Task.Delay(MinBatchDelayMs, ct);
		while (batch.Count < maxItems && !ct.IsCancellationRequested)
		{
			if (_channel.Reader.TryRead(out var item))
			{
				batch.Add(item);
			}
			else
			{
				// Если элементов нет, ждем минимальную задержку или завершение задачи задержки
				if (await Task.WhenAny(_channel.Reader.WaitToReadAsync(ct).AsTask(), delayTask) == delayTask)
				{
					break;
				}
			}
		}
	}
	private async Task WriteBatchWithRetryAsync(
		DatalakeContext db,
		ValuesRepository valuesRepository,
		List<ValueWriteRequest> batch,
		CancellationToken ct)
	{
		var attempt = 0;
		while (attempt < MaxRetryAttempts && !ct.IsCancellationRequested)
		{
			try
			{
				await valuesRepository.WriteCollectedValuesAsync(db, batch);
				logger.LogDebug("Записано {Count} значений", batch.Count);
				return; // Успешная запись
			}
			catch (Exception ex)
			{
				attempt++;
				logger.LogWarning(ex, "Ошибка записи (попытка {Attempt}/{Max})", attempt, MaxRetryAttempts);

				if (attempt < MaxRetryAttempts)
				{
					// Экспоненциальная задержка
					var delay = TimeSpan.FromMilliseconds(RetryBaseDelayMs * Math.Pow(2, attempt - 1));
					await Task.Delay(delay, ct);
				}
			}
		}

		if (attempt >= MaxRetryAttempts)
		{
			logger.LogError("Не удалось записать пачку из {Count} значений после {Max} попыток",
				batch.Count, MaxRetryAttempts);
		}
	}
}
