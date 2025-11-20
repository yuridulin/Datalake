using Datalake.Data.Application.Features.DataCollection.Commands.RestartCollection;
using Datalake.Data.Application.Features.UserAccess.Commands.Update;
using Datalake.Data.Application.Interfaces;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Application.Services;

[Singleton]
public class ApplicationStartService(
	IInfrastructureStartService infrastructureStartService,
	IServiceScopeFactory serviceScopeFactory,
	ILogger<ApplicationStartService> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Запущена инициализация работы приложения");

		try
		{
			await infrastructureStartService.StartAsync(stoppingToken);

			await using var scope = serviceScopeFactory.CreateAsyncScope();

			var accessHandler = scope.ServiceProvider.GetRequiredService<IUpdateUsersAccessHandler>();
			var restartHandler = scope.ServiceProvider.GetRequiredService<IRestartCollectionHandler>();

			var accessTask = accessHandler.HandleAsync(new() { Guids = [] }, stoppingToken);
			var restartTask = restartHandler.HandleAsync(new(), stoppingToken);

			await Task.WhenAll(accessTask, restartTask);

			logger.LogInformation("Приложение в работе");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Инициализация работы приложения не выполнена!");
			throw;
		}
	}
}
