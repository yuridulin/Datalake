using Datalake.Database;
using Datalake.Database.InMemory;
using Datalake.Database.Repositories;
using Datalake.PublicApi.Models.Values;
using System.Diagnostics.Metrics;
using System.Threading.Channels;

namespace Datalake.Server.Services.Collection;

/// <summary>
/// Служба записи новых значений в БД
/// </summary>
public class CollectorWriter : BackgroundService
{
	/// <summary>
	/// Служба записи новых значений в БД
	/// </summary>
	public CollectorWriter(
		DatalakeCurrentValuesStore currentValuesStore,
		IServiceScopeFactory serviceScopeFactory,
		ILogger<CollectorWriter> logger)
	{
		_currentValuesStore = currentValuesStore;
		_serviceScopeFactory = serviceScopeFactory;
		_logger = logger;

		// Gauge читает текущее число «висящих» в очереди элементов
		_queueLengthGauge = s_meter.CreateObservableGauge(
			"queue_length",
			() => new Measurement<long>(Interlocked.Read(ref _queuedItems)));
	}

	private const int BatchSize = 1000;
	private const int MaxBatchDelayMs = 250;
	private const int MaxRetryAttempts = 3;
	private const int RetryBaseDelayMs = 1000;

	private readonly Channel<ValueWriteRequest> _channel = Channel.CreateUnbounded<ValueWriteRequest>(new UnboundedChannelOptions
	{
		SingleWriter = false,
		SingleReader = true,
		AllowSynchronousContinuations = false
	});

	private readonly DatalakeCurrentValuesStore _currentValuesStore;
	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<CollectorWriter> _logger;

	// Метрики
	private static readonly Meter s_meter = new("Datalake.CollectorWriter", "1.0");
	private static readonly Counter<long> s_messagesProduced = s_meter.CreateCounter<long>("messages_produced");
	private static readonly Counter<long> s_messagesConsumed = s_meter.CreateCounter<long>("messages_consumed");
	private readonly ObservableGauge<long> _queueLengthGauge;
	private long _queuedItems;

	/// <summary>
	/// Основной consumer-loop: читает из канала, формирует батчи и записывает в БД
	/// </summary>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var batch = new List<ValueWriteRequest>(BatchSize);
		var lastFlush = DateTime.UtcNow;

		await foreach (var item in _channel.Reader.ReadAllAsync(stoppingToken))
		{
			batch.Add(item);

			var elapsed = (DateTime.UtcNow - lastFlush).TotalMilliseconds;
			if (batch.Count >= BatchSize || elapsed >= MaxBatchDelayMs)
			{
				await ProcessBatchAsync(batch, stoppingToken);
				batch.Clear();
				lastFlush = DateTime.UtcNow;
			}
		}

		// После Complete(): финальная запись оставшегося набора
		if (batch.Count > 0)
			await ProcessBatchAsync(batch, stoppingToken);
	}

	/// <summary>
	/// Пометка канала завершённым, чтобы consumer завершился
	/// </summary>
	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		_channel.Writer.Complete();
		await base.StopAsync(cancellationToken);
	}

	/// <summary>
	/// Producer: добавление новых значений в очередь
	/// </summary>
	public void AddToQueue(IEnumerable<ValueWriteRequest> values)
	{
		foreach (var value in values)
		{
			var exist = _currentValuesStore.Get(value.Id!.Value);
			var status = (exist!.Number != (value.Value as float?))
				? " (новое!)"
				: " (старое)";

			_logger.LogWarning(
					"Запись в очередь для БД:{Id} = {Val}{Status}",
					value.Id, value.Value, status);

			if (_channel.Writer.TryWrite(value))
			{
				Interlocked.Increment(ref _queuedItems);
				s_messagesProduced.Add(1);
			}
			else
			{
				_logger.LogCritical("Не удалось записать в канал!");
			}
		}
	}

	/// <summary>
	/// Обработка одного батча: запись с retry
	/// </summary>
	private async Task ProcessBatchAsync(
		List<ValueWriteRequest> batch,
		CancellationToken ct)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		using var db = scope.ServiceProvider.GetRequiredService<DatalakeContext>();
		var repo = scope.ServiceProvider.GetRequiredService<ValuesRepository>();

		await WriteBatchWithRetryAsync(db, repo, batch, ct);

		s_messagesConsumed.Add(batch.Count);
		Interlocked.Add(ref _queuedItems, -batch.Count);
	}

	/// <summary>
	/// Метод записи в БД с экспоненциальным бэкоффом
	/// </summary>
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
				return;
			}
			catch (Exception ex)
			{
				attempt++;
				_logger.LogWarning(ex,
					"Ошибка записи (попытка {Attempt}/{Max})",
					attempt, MaxRetryAttempts);

				if (attempt < MaxRetryAttempts)
				{
					var delay = TimeSpan.FromMilliseconds(
						RetryBaseDelayMs * Math.Pow(2, attempt - 1));
					await Task.Delay(delay, ct);
				}
			}
		}

		if (attempt >= MaxRetryAttempts)
		{
			_logger.LogError(
				"Не удалось записать пачку из {Count} значений после {Max} попыток",
				batch.Count, MaxRetryAttempts);
		}
	}
}
