using Datalake.Inventory.InMemory.Repositories;
using Datalake.InventoryService.Database;
using Datalake.InventoryService.InMemory.Stores;
using Datalake.PrivateApi.Utils;
using Datalake.PublicApi.Enums;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Initialization;

/// <summary>
/// Настройка БД
/// </summary>
public class DbInitializer(
	IServiceScopeFactory serviceScopeFactory,
	IConfiguration configuration,
	ILogger<DbExternalInitializer> logger)
{
	/// <summary>
	/// Настройка БД
	/// </summary>
	public async Task DoAsync()
	{
		logger.LogInformation("Настройка БД");

		try
		{
			using var serviceScope = serviceScopeFactory.CreateScope();

			// выполняем миграции через EF, хоть тут сгодится
			var ef = serviceScope.ServiceProvider.GetRequiredService<InventoryEfContext>();
			ef.Database.Migrate();

			// инициализируем подключение и стор
			var dataStore = serviceScope.ServiceProvider.GetRequiredService<DatalakeDataStore>();
			var usersRepository = serviceScope.ServiceProvider.GetRequiredService<UsersMemoryRepository>();

			// добавление пользователей по умолчанию
			var staticUsers = configuration.GetSection("StaticUsers").Get<StaticUsersOptions[]>();
			if (staticUsers == null)
			{
				logger.LogWarning("Список статичных учетных записей не прочитан");
			}
			else if (staticUsers.Length == 0)
			{
				logger.LogWarning("Список статичных учетных записей пуст");
			}
			else
			{
				logger.LogWarning("Список статичных учетных записей прочитан");

				EnvExpander.ExpandEnvVariables(staticUsers);
				await EnsureStaticUsersAsync(staticUsers.Select(x => (x.Name, x.Token, x.AccessType, x.Host)).ToArray());

				logger.LogWarning("Статичные учетные записи обновлены");
			}

			// начальное наполнение БД
			await EnsureDataCreatedAsync(usersRepository);

			logger.LogInformation("Настройка БД завершена");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Настройка БД не выполнена!");
			throw;
		}
	}

	internal class StaticUsersOptions
	{
		public required string Name { get; set; }

		public required string Token { get; set; }

		public required AccessType AccessType { get; set; }

		public string? Host { get; set; }
	}
}
