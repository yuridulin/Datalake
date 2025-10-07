using Datalake.Data.Application.Features.DataCollection.Commands.RestartCollection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Datalake.Data.Application.Services;

/// <summary>
/// Первый запуск системы сбора после старта приложения
/// </summary>
public class DataCollectionStartService(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var handler = scope.ServiceProvider.GetRequiredService<IRestartCollectionHandler>();

		await handler.HandleAsync(new(), stoppingToken);
	}
}
