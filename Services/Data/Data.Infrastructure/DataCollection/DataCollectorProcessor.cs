using Datalake.Domain.Enums;
using Datalake.Data.Application.Interfaces.DataCollection;
using Datalake.Data.Application.Models.Sources;
using Datalake.Shared.Application.Attributes;

namespace Datalake.Data.Infrastructure.DataCollection;

[Singleton]
public class DataCollectorProcessor(IDataCollectorFactory collectorsFactory) : IDataCollectorProcessor
{
	private readonly List<IDataCollector> collectors = new();
	private readonly SemaphoreSlim restartLock = new(1, 1);
	private CancellationTokenSource? globalCts;

	public async Task RestartAsync(IEnumerable<SourceSettingsDto> sources)
	{
		await restartLock.WaitAsync();

		try
		{
			await StopAsync();
			await StartAsync(sources);
		}
		finally
		{
			restartLock.Release();
		}
	}

	private async Task StartAsync(IEnumerable<SourceSettingsDto> sources)
	{
		foreach (var source in sources)
		{
			var collector = collectorsFactory.Create(source);
			if (collector != null)
			{
				collectors.Add(collector);
			}
		}

		globalCts = new();
		foreach (var collector in collectors)
			_ = Task.Run(() => collector.StartAsync(globalCts.Token));
	}

	private async Task StopAsync()
	{
		if (globalCts == null)
			return;

		globalCts.Cancel();

		if (collectors.Count == 0)
			return;

		try
		{
			await Task.WhenAll(collectors.Select(x => x.StopAsync()));
		}
		finally
		{
			collectors.Clear();
		}
	}
}
