using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Infrastructure.Interfaces;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Datalake.Inventory.Infrastructure.Database.Initialization;

/// <summary>
/// Настройка БД
/// </summary>
[Singleton]
public class DbInitializer(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<DbInitializer> logger) : BackgroundService
{
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		logger.LogInformation("Настройка БД");

		try
		{
			using var serviceScope = serviceScopeFactory.CreateScope();

			// выполняем миграции через EF
			var context = serviceScope.ServiceProvider.GetRequiredService<InventoryDbContext>();
			await context.Database.MigrateAsync(stoppingToken);

			// добавление пользователей по умолчанию
			/*var staticUsersOptions = configuration.GetSection("StaticUsers").Get<StaticUsersOptionsDto[]>();
			if (staticUsersOptions == null)
			{
				logger.LogWarning("Список статичных учетных записей не прочитан");
			}
			else if (staticUsersOptions.Length == 0)
			{
				logger.LogWarning("Список статичных учетных записей пуст");
			}
			else
			{
				logger.LogWarning("Список статичных учетных записей прочитан");

				EnvExpander.ExpandEnvVariables(staticUsersOptions);

				var staticUsers = staticUsersOptions
					.ToDictionary(x => x.Name + x.Token, x => UserEntity.CreateFromStaticOptions(name: x.Name, token: x.Token, host: x.Host));

				// нужно обновить токены в существующих и добавить недостающие

				var existsStaticUsers = await context.Users
					.AsNoTracking()
					.Select(u => u.FullName + u.PasswordHash)
					.Where(h => !staticUsers.Keys.Contains(h))
					.ToArrayAsync();

				await context.Users.AddRangeAsync(staticUsers
					.Where(kv => !existsStaticUsers.Contains(kv.Key))
					.Select(x => x.Value));
				await context.SaveChangesAsync();

				logger.LogWarning("Статичные учетные записи обновлены");
			}*/

			// запись необходимых источников в список
			var customSources = Source.CustomSources
				.Select(x => Source.CreateAsInternal(type: x, name: x.ToString(), description: GetDescription(x)))
				.ToArray();

			var existsCustomSources = await context.Sources
				.Where(x => customSources.Select(c => c.Id).Contains(x.Id))
				.Select(x => x.Id)
				.ToArrayAsync(stoppingToken);

			await context.Sources.AddRangeAsync(customSources.ExceptBy(existsCustomSources, x => x.Id), stoppingToken);
			await context.SaveChangesAsync(stoppingToken);

			// создание таблицы настроек
			if (!await context.Settings.AnyAsync(stoppingToken))
			{
				var setting = new Settings(string.Empty, string.Empty, string.Empty, string.Empty);

				await context.Settings.AddAsync(setting, stoppingToken);
				await context.SaveChangesAsync(stoppingToken);
			}

			// создание администратора по умолчанию, если его учетки нет
			if (!await context.Users.AnyAsync(x => x.Login == "admin", stoppingToken))
			{
				var admin = User.CreateFromLoginPassword("admin", "admin");

				await context.Users.AddAsync(admin, stoppingToken);
				await context.SaveChangesAsync(stoppingToken);
			}

			// Загрузка сессий пользователей
			var auditLog = new Log(LogCategory.Core, "Сервер запущен", null);
			await context.SaveChangesAsync(stoppingToken);

			logger.LogInformation("Настройка БД завершена");

			// после настройки БД инициализируем кэши
			var inventoryCache = serviceScope.ServiceProvider.GetRequiredService<IInventoryCache>();
			var energoIdCache = serviceScope.ServiceProvider.GetRequiredService<IEnergoIdCache>();
			var userAccessCache = serviceScope.ServiceProvider.GetRequiredService<IUserAccessCache>();

			// когда все кэши в работе, подгружаем БД в основной кэш
			await inventoryCache.RestoreAsync();

			var energoIdViewCreator = serviceScope.ServiceProvider.GetRequiredService<IEnergoIdViewCreator>();
			await energoIdViewCreator.RecreateAsync(stoppingToken);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Настройка БД не выполнена!");
			throw;
		}
	}

	/// <summary>
	/// Получение описания значения enum
	/// </summary>
	/// <param name="enumValue">Значение</param>
	/// <returns>Описание, записанное в атрибуте Description</returns>
	static string GetDescription(Enum enumValue)
	{
		var field = enumValue.GetType().GetField(enumValue.ToString());
		if (field == null)
			return enumValue.ToString();

		if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
			return attribute.Description;

		return enumValue.ToString();
	}
}
