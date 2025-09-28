using Datalake.InventoryService.Domain.Extensions;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Cache.Inventory.Services;
using Datalake.InventoryService.Infrastructure.Database;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Settings;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Application.Features.Settings;

/// <summary>
/// Репозиторий работы с настройками в памяти приложения
/// </summary>
public class SettingsMemoryRepository(InventoryCacheStore dataStore)
{
	#region API

	/// <summary>
	/// Получение настроек приложения
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Настройки</returns>
	public SettingsInfo GetSettings(UserAccessEntity? user)
	{
		user?.ThrowIfNoGlobalAccess(AccessType.Admin);

		return dataStore.State.SettingsInfo();
	}

	/// <summary>
	/// Изменение настроек приложения
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="newSettings">Новые настройки</param>
	public async Task UpdateSettingsAsync(
		InventoryEfContext db, UserAccessEntity user, SettingsInfo newSettings)
	{
		user.ThrowIfNoGlobalAccess(AccessType.Admin);

		await ProtectedUpdateSettingsAsync(db, user.Guid, newSettings);
	}

	#endregion

	#region Действия

	internal async Task ProtectedUpdateSettingsAsync(
		InventoryEfContext db, Guid userGuid, SettingsInfo request)
	{
		// Проверки, не требующие стейта

		// Блокируем стейт до завершения обновления
		InventoryState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			var newSettings = currentState.Settings with
			{
				KeycloakHost = request.EnergoIdHost,
				KeycloakClient = request.EnergoIdClient,
				EnergoIdApi = request.EnergoIdApi,
				InstanceName = request.InstanceName,
				LastUpdate = DateTime.UtcNow,
			};

			// Обновление в БД
			using var transaction = await db.Database.BeginTransactionAsync();
			try
			{
				await db.Settings
					.ExecuteUpdateAsync(x => x
						.SetProperty(p => p.KeycloakHost, newSettings.KeycloakHost)
						.SetProperty(p => p.KeycloakClient, newSettings.KeycloakClient)
						.SetProperty(p => p.EnergoIdApi, newSettings.EnergoIdApi)
						.SetProperty(p => p.InstanceName, newSettings.InstanceName));

				await db.Audit.AddAsync(new Log(
					LogCategory.Core,
					LogType.Success,
					userGuid,
					"Изменены настройки",
					ObjectExtension.Difference(currentState.Settings, newSettings)));

				await db.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось обновить настройки в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Settings = newSettings,
			});
		}

		// Возвращение ответа
	}

	#endregion
}
