namespace Datalake.Gateway.Application.Interfaces;

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
