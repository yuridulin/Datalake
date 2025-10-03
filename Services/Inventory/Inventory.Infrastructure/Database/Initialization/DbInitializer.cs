using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Infrastructure.Cache.Inventory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.ComponentModel;

namespace Datalake.Inventory.Infrastructure.Database.Initialization;

/// <summary>
/// Настройка БД
/// </summary>
public class DbInitializer(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<EnergoIdViewCreator> logger)
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
			var context = serviceScope.ServiceProvider.GetRequiredService<InventoryEfContext>();
			context.Database.Migrate();

			// инициализируем подключение и стор
			var dataStore = serviceScope.ServiceProvider.GetRequiredService<InventoryCacheStore>();

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
			var customSources = SourceEntity.CustomSources
				.Select(x => new SourceEntity(type: x, name: x.ToString(), description: GetDescription(x), address: null))
				.ToArray();

			var existsCustomSources = await context.Sources
				.Where(x => customSources.Select(c => c.Id).Contains(x.Id))
				.Select(x => x.Id)
				.ToArrayAsync();

			await context.Sources.AddRangeAsync(customSources.ExceptBy(existsCustomSources, x => x.Id));
			await context.SaveChangesAsync();

			// создание таблицы настроек
			if (!await context.Settings.AnyAsync())
			{
				var setting = new SettingsEntity(string.Empty, string.Empty, string.Empty, string.Empty);

				await context.Settings.AddAsync(setting);
				await context.SaveChangesAsync();
			}

			// создание администратора по умолчанию, если его учетки нет
			if (await context.Users.AnyAsync(x => x.Login == "admin"))
			{
				var admin = UserEntity.CreateFromLoginPassword("admin", "admin");

				await context.Users.AddAsync(admin);
				await context.SaveChangesAsync();
			}

			// Загрузка сессий пользователей
			var auditLog = new AuditEntity(LogCategory.Core, "Сервер запущен", null);
			await context.SaveChangesAsync();

			logger.LogInformation("Настройка БД завершена");
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
