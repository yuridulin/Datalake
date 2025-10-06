using Datalake.Data.Application.DataCollection.Interfaces;
using Datalake.Data.Application.DataCollection.Repositories;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Diagnostics.Metrics;
using System.Threading.Channels;

namespace Datalake.Data.Infrastructure.DataCollection;

[Singleton]
public class DataWriterService : BackgroundService, IDataWriterService
{
	/// <summary>
	/// Служба записи новых значений в БД
	/// </summary>
	public DataWriterService(
		IServiceScopeFactory serviceScopeFactory,
		ILogger<DataWriterService> logger)
	{
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

	private readonly Channel<TagHistory> _channel = Channel.CreateUnbounded<TagHistory>(new UnboundedChannelOptions
	{
		SingleWriter = false,
		SingleReader = true,
		AllowSynchronousContinuations = false
	});

	private readonly IServiceScopeFactory _serviceScopeFactory;
	private readonly ILogger<DataWriterService> _logger;

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
		var batch = new List<TagHistory>(BatchSize);
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
	public void AddToQueue(IEnumerable<TagHistory> values)
	{
		foreach (var value in values)
		{
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
		List<TagHistory> batch,
		CancellationToken ct)
	{
		using var scope = _serviceScopeFactory.CreateScope();
		var repository = scope.ServiceProvider.GetRequiredService<ITagsHistoryRepository>();

		await WriteBatchWithRetryAsync(repository, batch, ct);

		s_messagesConsumed.Add(batch.Count);
		Interlocked.Add(ref _queuedItems, -batch.Count);
	}

	/// <summary>
	/// Метод записи в БД с экспоненциальным бэкоффом
	/// </summary>
	private async Task WriteBatchWithRetryAsync(
		ITagsHistoryRepository repository,
		List<TagHistory> batch,
		CancellationToken ct)
	{
		var attempt = 0;
		while (attempt < MaxRetryAttempts && !ct.IsCancellationRequested)
		{
			try
			{
				await repository.WriteAsync(batch); // TODO: команда на запись?
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