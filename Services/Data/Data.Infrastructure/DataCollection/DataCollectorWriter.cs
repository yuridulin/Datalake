using Datalake.Data.Application.Features.Values.Commands.SystemWriteValues;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading.Channels;

namespace Datalake.Data.Infrastructure.DataCollection;

/// <summary>
/// Служба записи новых значений в БД
/// </summary>
[Singleton]
public class DataCollectorWriter(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<DataCollectorWriter> logger) : BackgroundService, IDataCollectorWriter
{
	private const int BatchSize = 1000;
	private const int MaxBatchDelayMs = 250;
	private const int MaxRetryAttempts = 3;
	private const int RetryBaseDelayMs = 1000;

	private readonly Channel<TagValue> channel = Channel.CreateUnbounded<TagValue>(new UnboundedChannelOptions
	{
		SingleWriter = false,
		SingleReader = true,
		AllowSynchronousContinuations = false
	});

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		var batch = new List<TagValue>(BatchSize);
		var lastFlush = DateTime.UtcNow;

		await foreach (var item in channel.Reader.ReadAllAsync(stoppingToken))
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

		// финальная запись оставшегося набора
		if (batch.Count > 0)
			await ProcessBatchAsync(batch, stoppingToken);
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		channel.Writer.Complete();
		await base.StopAsync(cancellationToken);
	}

	public async Task AddValuesToQueueAsync(IReadOnlyCollection<TagValue> values, CancellationToken cancellationToken = default)
	{
		foreach (var value in values)
		{
			try
			{
				await channel.Writer.WriteAsync(value, cancellationToken);
			}
			catch(OperationCanceledException)
			{
				// Игнорируем - система останавливается
				break;
			}
		}
	}

	private async Task ProcessBatchAsync(
		List<TagValue> batch,
		CancellationToken ct)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var systemWriteValuesHandler = scope.ServiceProvider.GetRequiredService<ISystemWriteValuesHandler>();

		await WriteBatchWithRetryAsync(systemWriteValuesHandler, batch, ct);
	}

	private async Task WriteBatchWithRetryAsync(
		ISystemWriteValuesHandler systemWriteValuesHandler,
		List<TagValue> batch,
		CancellationToken ct)
	{
		var attempt = 0;
		while (attempt < MaxRetryAttempts && !ct.IsCancellationRequested)
		{
			try
			{
				await systemWriteValuesHandler.HandleAsync(new() { Values = batch }, ct);
				return;
			}
			catch (Exception ex)
			{
				attempt++;
				logger.LogWarning(ex,
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
			logger.LogError(
				"Не удалось записать пачку из {Count} значений после {Max} попыток",
				batch.Count, MaxRetryAttempts);
		}
	}
}