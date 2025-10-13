using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Infrastructure.Interfaces;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Infrastructure.Database.Initialization;

/// <summary>
/// Настройка БД
/// </summary>
[Singleton]
public class InfrastructureStartService(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<InfrastructureStartService> logger) : IInfrastructureStartService
{
	public async Task StartAsync()
	{
		logger.LogInformation("Запущена инициализация работы с БД");

		try
		{
			using var serviceScope = serviceScopeFactory.CreateScope();

			// выполняем миграции через EF
			var context = serviceScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
			await context.Database.MigrateAsync();

			// создание представления пользователей EnergoId
			var viewCreator = serviceScope.ServiceProvider.GetRequiredService<IEnergoIdViewCreator>();
			await viewCreator.RecreateAsync();

			logger.LogInformation("Инициализация работы с БД выполнена");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Инициализация работы с БД не выполнена!");
			throw;
		}
	}
}
