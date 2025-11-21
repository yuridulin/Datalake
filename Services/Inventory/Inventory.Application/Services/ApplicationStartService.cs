using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Services;

[Singleton]
public class ApplicationStartService(
	IInfrastructureStartService infrastructureStartService,
	IDomainStartService domainStartService,
	IInventoryStore inventoryCache,
	IEnergoIdStore energoIdCache,
	IUserAccessSynchronizationService userAccessSynchronizationService,
	ILogger<ApplicationStartService> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Запущена инициализация работы приложения");

		try
		{
			// нужно выполнить инициализацию БД
			await infrastructureStartService.StartAsync(stoppingToken);

			// создать необходимые записи
			await domainStartService.StartAsync();

			// загрузить и создать начальное состояние для кэша пользователей EnergoId (если не откажемся от него)
			energoIdCache.SetReady();

			// настроить синхронизацию кэша прав вслед за кэшем структуры
			userAccessSynchronizationService.Start();

			// загрузить и создать начальное состояние для кэша структуры
			await inventoryCache.RestoreAsync();

			logger.LogInformation("Приложение в работе");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Инициализация работы приложения не выполнена!");
			throw;
		}
	}
}
