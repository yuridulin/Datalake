using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.DataCollection;

[Singleton]
public class DataCollectorProcessor(
	IDataCollectorFactory collectorsFactory,
	IDataCollectorWriter dataWriter,
	ILogger<DataCollectorProcessor> logger) : IDataCollectorProcessor
{
	private readonly List<IDataCollector> collectors = new();
	private readonly CancellationTokenSource globalCts = new();
	private readonly SemaphoreSlim restartLock = new(1, 1);

	public async Task RestartAsync(IEnumerable<SourceSettingsDto> sources)
	{
		await restartLock.WaitAsync();

		try
		{
			await StartAsync(sources);
		}
		finally
		{
			restartLock.Release();
		}
	}

	private async Task StartAsync(IEnumerable<SourceSettingsDto> sources)
	{
		logger.LogInformation("Запуск системы сбора данных");

		// Останавливаем текущие сборщики
		await StopAsync();

		// Создаём новые сборщики
		collectors.Clear();
		foreach (var source in sources)
		{
			var collector = collectorsFactory.Create(source);
			if (collector != null)
			{
				collectors.Add(collector);
			}
		}

		// Запускаем новые сборщики и обработчики их каналов
		foreach (var collector in collectors)
			await collector.StartAsync(globalCts.Token);

		logger.LogInformation("Система сбора данных запущена");
	}

	private async Task StopAsync()
	{
		if (collectors.Count == 0)
			return;

		logger.LogInformation("Остановка системы сбора данных");

		var stopTasks = new List<Task>();
		foreach (var collector in collectors)
			stopTasks.Add(Task.Run(collector.StopAsync));

		var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
		var completedTask = await Task.WhenAny(Task.WhenAll(stopTasks), timeoutTask);

		if (completedTask == timeoutTask)
		{
			logger.LogWarning("Таймаут остановки обработчиков!");
		}

		collectors.Clear();
		logger.LogInformation("Система сбора данных остановлена");
	}

	public async Task WriteValuesAsync(IReadOnlyCollection<TagValue> values, CancellationToken cancellationToken = default)
	{
		await dataWriter.AddValuesToQueueAsync(values, cancellationToken);
	}
}
