using Datalake.Gateway.Application.Features.UserAccess.Commands.Update;
using Datalake.Gateway.Application.Interfaces;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Datalake.Gateway.Application.Services;

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
			await accessHandler.HandleAsync(new() { Guids = [], IsAllUsers = true }, stoppingToken);

			logger.LogInformation("Приложение в работе");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Инициализация работы приложения не выполнена!");
			throw;
		}
	}
}
