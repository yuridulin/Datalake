using Datalake.Database.InMemory;
using Datalake.Database.InMemory.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Datalake.Database.Initialization;

/// <summary>
/// Настройка БД
/// </summary>
public class DbInitializer(
	IServiceScopeFactory serviceScopeFactory,
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
			var ef = serviceScope.ServiceProvider.GetRequiredService<DatalakeEfContext>();
			ef.Database.Migrate();

			// инициализируем подключение и стор
			var db = serviceScope.ServiceProvider.GetRequiredService<DatalakeContext>();
			var dataStore = serviceScope.ServiceProvider.GetRequiredService<DatalakeDataStore>();
			var usersRepository = serviceScope.ServiceProvider.GetRequiredService<UsersMemoryRepository>();

			// начальное наполнение БД
			await db.EnsureDataCreatedAsync(dataStore, usersRepository);

			logger.LogInformation("Настройка БД завершена");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Настройка БД не выполнена!");
			throw;
		}
	}
}
