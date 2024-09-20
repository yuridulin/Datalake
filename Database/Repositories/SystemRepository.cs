using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Settings;
using Datalake.ApiClasses.Models.Users;
using Datalake.Database.Repositories.Base;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class SystemRepository(DatalakeContext db) : RepositoryBase
{
	#region Действия

	public async Task<SettingsInfo> GetSettingsAsync()
	{
		var setting = await db.Settings
			.FirstOrDefaultAsync();

		if (setting == null)
		{
			await db.EnsureDataCreatedAsync();

			setting = await db.Settings
				.FirstAsync();
		}

		return new SettingsInfo
		{
			EnergoIdHost = setting.KeycloakHost,
			EnergoIdClient = setting.KeycloakClient,
			EnergoIdApi = setting.EnergoIdApi,
		};
	}

	public async Task UpdateSettingsAsync(UserAuthInfo user, SettingsInfo newSettings)
	{
		CheckGlobalAccess(user, AccessType.Admin);

		await UpdateSettingsAsync(newSettings);
	}

	#endregion

	#region Реализация

	internal async Task UpdateSettingsAsync(SettingsInfo newSettings)
	{
		try
		{
			await db.Settings
				.Set(x => x.KeycloakHost, newSettings.EnergoIdHost)
				.Set(x => x.KeycloakClient, newSettings.EnergoIdClient)
				.Set(x => x.EnergoIdApi, newSettings.EnergoIdApi)
				.UpdateAsync();
		}
		catch (Exception ex)
		{
			throw new DatabaseException(message: "не удалось изменить настройки", ex);
		}
	}

	#endregion
}
