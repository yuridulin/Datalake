using Datalake.Database.Constants;
using Datalake.Database.Enums;
using Datalake.Database.Exceptions;
using Datalake.Database.Extensions;
using Datalake.Database.Models.Auth;
using Datalake.Database.Models.Settings;
using Datalake.Database.Models.Tags;
using Datalake.Database.Tables;
using LinqToDB;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с настройками и кэшами
/// </summary>
/// <param name="db"></param>
public partial class SystemRepository(DatalakeContext db)
{
	#region Действия

	/// <summary>
	/// Получение настроек приложения
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Настройки</returns>
	public async Task<SettingsInfo> GetSettingsAsync(UserAuthInfo user)
	{
		AccessRepository.CheckGlobalAccess(user.Rights, AccessType.Admin);

		return await GetSettingsAsync();
	}

	/// <summary>
	/// Изменение настроек приложения
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="newSettings">Новые настройки</param>
	public async Task UpdateSettingsAsync(UserAuthInfo user, SettingsInfo newSettings)
	{
		AccessRepository.CheckGlobalAccess(user.Rights, AccessType.Admin);
		User = user.Guid;

		await UpdateSettingsAsync(newSettings);
	}

	/// <summary>
	/// Перестроение кэша системы получения данных
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	public async Task RebuildStorageCacheAsync(UserAuthInfo user)
	{
		AccessRepository.CheckGlobalAccess(user.Rights, AccessType.Admin);
		User = user.Guid;

		await RebuildStorageCacheAsync();

		await db.InsertAsync(new Log
		{
			Category = LogCategory.Core,
			Type = LogType.Success,
			Text = "Перезапуск служб получения данных",
			UserGuid = User,
		});
	}


	/// <summary>
	/// Получение настроек приложения от имени приложения
	/// </summary>
	/// <returns>Настройки</returns>
	public async Task<SettingsInfo> GetSettingsAsSystemAsync()
	{
		return await GetSettingsAsync();
	}

	#endregion

	#region Реализация

	Guid? User { get; set; }

	internal async Task<SettingsInfo> GetSettingsAsync()
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
			InstanceName = setting.InstanceName,
		};
	}

	internal async Task UpdateSettingsAsync(SettingsInfo newSettings)
	{
		try
		{
			var settings = await GetSettingsAsync();

			await db.Settings
				.Set(x => x.KeycloakHost, newSettings.EnergoIdHost)
				.Set(x => x.KeycloakClient, newSettings.EnergoIdClient)
				.Set(x => x.EnergoIdApi, newSettings.EnergoIdApi)
				.Set(x => x.InstanceName, newSettings.InstanceName)
				.UpdateAsync();

			await db.InsertAsync(new Log
			{
				Category = LogCategory.Core,
				Type = LogType.Success,
				Text = "Изменены настройки",
				UserGuid = User,
				Details = ObjectExtension.Difference(settings, newSettings),
			});
		}
		catch (Exception ex)
		{
			throw new DatabaseException(message: "не удалось изменить настройки", ex);
		}
	}

	#endregion

	#region Кэш

	/// <summary>
	/// Время последнего обновления кэша получения данных
	/// </summary>
	public static DateTime LastUpdate { get; set; } = DateTime.MinValue;

	/// <summary>
	/// Обновление времени последнего изменения кэша.
	/// По обновлению этого времени служба, изменяющая кэш, понимает, что нужно его изменить
	/// </summary>
	internal static void Update()
	{
		lock (locker)
		{
			LastUpdate = DateFormats.GetCurrentDateTime();
		}
	}

	internal async Task RebuildStorageCacheAsync()
	{
		var tables = await db.TablesRepository.GetHistoryTablesFromSchema();

		TablesRepository.CachedTables = tables
			.Where(x => x.Name.StartsWith(TablesRepository.NamePrefix))
			.Select(x => new
			{
				Date = TablesRepository.GetTableDate(x.Name),
				x.Name,
			})
			.Where(x => x.Date != DateTime.MinValue)
			.DistinctBy(x => x.Date)
			.ToDictionary(x => x.Date, x => x.Name);

		TagsRepository.CachedTags = await (
			from t in db.Tags
			from s in db.Sources.LeftJoin(x => x.Id == t.SourceId)
			select new TagCacheInfo
			{
				Id = t.Id,
				Guid = t.GlobalGuid,
				Name = t.Name,
				TagType = t.Type,
				SourceType = s.Type,
				IsManual = t.SourceId == (int)CustomSource.Manual,
				ScalingCoefficient = t.IsScaling
					? ((t.MaxEu - t.MinEu) / (t.MaxRaw - t.MinRaw))
					: 1,
			}
		).ToDictionaryAsync(x => x.Id, x => x);

		// создание таблицы для значений на текущую дату
		if (!TablesRepository.CachedTables.ContainsKey(DateTime.Today))
		{
			db.TablesRepository.GetHistoryTable(DateTime.Today);
		}

		// актуализация таблицы текущих значений
		await db.ValuesRepository.CreateLiveValues();
	}

	static object locker = new();

	#endregion
}
