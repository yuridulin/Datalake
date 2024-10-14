using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Settings;
using Datalake.ApiClasses.Models.Tags;
using Datalake.ApiClasses.Models.Users;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class SystemRepository(DatalakeContext db)
{
	public static DateTime LastUpdate { get; set; } = DateTime.MinValue;

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
		await db.AccessRepository.CheckGlobalAccess(user, AccessType.Admin);

		await UpdateSettingsAsync(newSettings);
	}

	public async Task RebuildCacheAsync(UserAuthInfo user)
	{
		await db.AccessRepository.CheckGlobalAccess(user, AccessType.Admin);
		await RebuildCacheAsync();
	}

	public static void Update()
	{
		lock (locker)
		{
			LastUpdate = DateTime.Now;
		}
	}

	#endregion


	#region Реализация

	static object locker = new();
	internal async Task RebuildCacheAsync()
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
		var lastValues = await db.ValuesRepository.ReadHistoryValuesAsync([.. TagsRepository.CachedTags.Keys], DateTime.Now, DateTime.Now);

		ValuesRepository.WriteLiveValues(lastValues);
	}

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
