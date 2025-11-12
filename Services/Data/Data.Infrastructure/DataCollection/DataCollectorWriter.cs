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

	private CancellationToken globalToken;
	private readonly List<TagValue> batch = new(BatchSize);
	private readonly Channel<TagValue> channel = Channel.CreateUnbounded<TagValue>(new UnboundedChannelOptions
	{
		SingleWriter = false,
		SingleReader = true,
		AllowSynchronousContinuations = false
	});

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		globalToken = stoppingToken;

		var lastFlush = DateTime.UtcNow;
		await foreach (var item in channel.Reader.ReadAllAsync(globalToken))
		{
			batch.Add(item);

			var elapsed = (DateTime.UtcNow - lastFlush).TotalMilliseconds;
			if (batch.Count >= BatchSize || elapsed >= MaxBatchDelayMs)
			{
				await ProcessBatchAsync(); // ждем запись, а новые значения накапливаются в канале
				batch.Clear();
				lastFlush = DateTime.UtcNow;
			}
		}

		// финальная запись оставшегося набора
		if (batch.Count > 0)
			await ProcessBatchAsync();
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		channel.Writer.Complete();
		await base.StopAsync(cancellationToken);
	}

	public ValueTask WriteAsync(TagValue value) => channel.Writer.WriteAsync(value, globalToken);

	private async Task ProcessBatchAsync()
	{
		using var scope = serviceScopeFactory.CreateScope();
		var systemWriteValuesHandler = scope.ServiceProvider.GetRequiredService<ISystemWriteValuesHandler>();

		await WriteBatchWithRetryAsync(systemWriteValuesHandler);
	}

	private async Task WriteBatchWithRetryAsync(ISystemWriteValuesHandler systemWriteValuesHandler)
	{
		var attempt = 0;
		while (attempt < MaxRetryAttempts && !globalToken.IsCancellationRequested)
		{
			try
			{
				// отправляем команду, а не просто пишем в реп
				// команда выполнит обновление кэша и другие связанные операции
				await systemWriteValuesHandler.HandleAsync(new() { Values = batch }, globalToken);
				return;
			}
			catch (Exception ex)
			{
				attempt++;
				logger.LogWarning(ex, "Ошибка записи: попытка {attempt} из {max}", attempt, MaxRetryAttempts);

				if (attempt < MaxRetryAttempts)
				{
					var delay = TimeSpan.FromMilliseconds(RetryBaseDelayMs * Math.Pow(2, attempt - 1));
					await Task.Delay(delay, globalToken);
				}
			}
		}

		if (attempt >= MaxRetryAttempts)
		{
			logger.LogError("Не удалось записать пачку из {count} значений после {max} попыток", batch.Count, MaxRetryAttempts);
		}
	}
}