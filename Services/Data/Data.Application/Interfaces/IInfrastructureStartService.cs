namespace Datalake.Data.Application.Interfaces;

/// <summary>
/// Настройка
/// </summary>
public interface IInfrastructureStartService
{
	/// <summary>
	/// Настройка
	/// </summary>
	Task StartAsync(CancellationToken stoppingToken);
}
