namespace Datalake.Data.Application.Interfaces.DataCollection;

public interface IDataCollector
{
	string Name { get; }

	Task StartAsync(CancellationToken stoppingToken = default);

	Task StopAsync();
}
