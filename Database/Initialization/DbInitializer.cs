using Datalake.Database.Functions;
using Datalake.Database.InMemory.Repositories;
using Datalake.Database.InMemory.Stores;
using Datalake.Database.InMemory.Stores.Derived;
using Datalake.PublicApi.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Datalake.Database.Initialization;

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
			var ef = serviceScope.ServiceProvider.GetRequiredService<DatalakeEfContext>();
			ef.Database.Migrate();

			// инициализируем подключение и стор
			var db = serviceScope.ServiceProvider.GetRequiredService<DatalakeContext>();
			var dataStore = serviceScope.ServiceProvider.GetRequiredService<DatalakeDataStore>();
			var usersRepository = serviceScope.ServiceProvider.GetRequiredService<UsersMemoryRepository>();
			var sessionsStore = serviceScope.ServiceProvider.GetRequiredService<DatalakeSessionsStore>();

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
				await db.EnsureStaticUsersAsync(staticUsers.Select(x => (x.Name, x.Token, x.AccessType, x.Host)).ToArray());

				logger.LogWarning("Статичные учетные записи обновлены");
			}

			// начальное наполнение БД
			await db.EnsureDataCreatedAsync(usersRepository, sessionsStore);

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
