using Datalake.Shared.Application.Attributes;
using Microsoft.Extensions.Hosting;

namespace Datalake.Inventory.Application.Services;

[Singleton]
public class ApplicationStartService : BackgroundService
{
	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		// нужно выполнить инициализацию БД
		// создать необходимые записи
		// загрузить и создать начальное состояние для кэша структуры
		// загрузить и создать начальное состояние для кэша прав
		// загрузить и создать начальное состояние для кэша пользователей EnergoId (если не откажемся от него)

		return Task.CompletedTask;
	}
}
