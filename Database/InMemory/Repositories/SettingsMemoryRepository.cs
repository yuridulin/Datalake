using Datalake.Database.Extensions;
using Datalake.Database.InMemory.Models;
using Datalake.Database.InMemory.Queries;
using Datalake.Database.Repositories;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Settings;
using LinqToDB;

namespace Datalake.Database.InMemory.Repositories;

/// <summary>
/// Репозиторий работы с настройками в памяти приложения
/// </summary>
public class SettingsMemoryRepository(DatalakeDataStore dataStore)
{
	#region Действия

	/// <summary>
	/// Получение настроек приложения
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Настройки</returns>
	public SettingsInfo GetSettings(UserAuthInfo? user)
	{
		if (user != null)
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return dataStore.State.SettingsInfo();
	}

	/// <summary>
	/// Изменение настроек приложения
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="newSettings">Новые настройки</param>
	public async Task UpdateSettingsAsync(
		DatalakeContext db, UserAuthInfo user, SettingsInfo newSettings)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		await ProtectedUpdateSettingsAsync(db, user.Guid, newSettings);
	}

	#endregion

	internal async Task ProtectedUpdateSettingsAsync(
		DatalakeContext db, Guid userGuid, SettingsInfo request)
	{
		// Проверки, не требующие стейта

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
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
			using var transaction = await db.BeginTransactionAsync();
			try
			{
				await db.Settings
					.Set(x => x.KeycloakHost, newSettings.KeycloakHost)
					.Set(x => x.KeycloakClient, newSettings.KeycloakClient)
					.Set(x => x.EnergoIdApi, newSettings.EnergoIdApi)
					.Set(x => x.InstanceName, newSettings.InstanceName)
					.UpdateAsync();

				await db.InsertAsync(new Log
				{
					Category = LogCategory.Core,
					Type = LogType.Success,
					Text = "Изменены настройки",
					AuthorGuid = userGuid,
					Details = ObjectExtension.Difference(currentState.Settings, newSettings),
				});

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось создать тег в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Settings = newSettings,
			});
		}

		// Возвращение ответа
	}
}
