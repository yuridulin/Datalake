using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Hosting;

namespace Datalake.Inventory.Application.Services;

[Singleton]
public class ApplicationStartService(
	IInfrastructureStartService infrastructureStartService,
	IDomainStartService domainStartService,
	IInventoryCache inventoryCache,
	IEnergoIdCache energoIdCache,
	IUserAccessSynchronizationService userAccessSynchronizationService) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// нужно выполнить инициализацию БД
		await infrastructureStartService.StartAsync();

		// создать необходимые записи
		await domainStartService.StartAsync();

		// загрузить и создать начальное состояние для кэша структуры
		await inventoryCache.RestoreAsync();

		// загрузить и создать начальное состояние для кэша пользователей EnergoId (если не откажемся от него)
		energoIdCache.SetReady();

		// все кэши готовы, запускаем синхронизацию кэша прав вслед за кэшем структуры
		userAccessSynchronizationService.Start();
	}
}
