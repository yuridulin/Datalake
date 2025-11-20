using Datalake.Data.Application.Interfaces;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Infrastructure.Database.Services;

/// <summary>
/// Настройка БД
/// </summary>
[Singleton]
public class InfrastructureStartService(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<InfrastructureStartService> logger) : IInfrastructureStartService
{
	public async Task StartAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Запущена инициализация работы с БД");

		try
		{
			await using var serviceScope = serviceScopeFactory.CreateAsyncScope();

			// выполняем миграции через EF
			var context = serviceScope.ServiceProvider.GetRequiredService<DataDbContext>();
			await context.Database.MigrateAsync(stoppingToken);

			logger.LogInformation("Инициализация работы с БД выполнена");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Инициализация работы с БД не выполнена!");
			throw;
		}
	}
}
