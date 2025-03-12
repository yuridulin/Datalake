using Datalake.Database.Extensions;
using Datalake.Database.Tables;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.LogModels;
using Datalake.PublicApi.Models.Settings;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.Users;
using LinqToDB;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с настройками и кэшами
/// </summary>
/// <param name="db"></param>
public class SystemRepository(DatalakeContext db)
{
	#region Действия

	/// <summary>
	/// Получение списка сообщений
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="take">Сколько сообщений получить за этот запрос</param>
	/// <param name="lastId">Идентификатор сообщения, с которого начать отсчёт количества</param>
	/// <param name="sourceId">Идентификатор затронутого источника</param>
	/// <param name="blockId">Идентификатор затронутого блока</param>
	/// <param name="tagGuid">Идентификатор затронутого тега</param>
	/// <param name="userGuid">Идентификатор затронутого пользователя</param>
	/// <param name="groupGuid">Идентификатор затронутой группы пользователей</param>
	/// <param name="categories">Выбранные категории сообщений</param>
	/// <param name="types">Выбранные типы сообщений</param>
	/// <param name="authorGuid">Идентификатор пользователя, создавшего сообщение</param>
	/// <returns>Список сообщений</returns>
	public async Task<LogInfo[]> GetLogsAsync(
		UserAuthInfo user,
		int? take = null,
		int? lastId = null,
		int? sourceId = null,
		int? blockId = null,
		Guid? tagGuid = null,
		Guid? userGuid = null,
		Guid? groupGuid = null,
		LogCategory[]? categories = null,
		LogType[]? types = null,
		Guid? authorGuid = null)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Editor);

		var query = QueryLogs();

		if (lastId.HasValue)
			query = query.Where(x => x.Id > lastId.Value);

		if (sourceId.HasValue)
			query = query.Where(x => x.RefId == sourceId.Value.ToString());

		if (blockId.HasValue)
			query = query.Where(x => x.RefId == blockId.Value.ToString());

		if (tagGuid.HasValue)
			query = query.Where(x => x.RefId == tagGuid.Value.ToString());

		if (userGuid.HasValue)
			query = query.Where(x => x.RefId == userGuid.Value.ToString());

		if (groupGuid.HasValue)
			query = query.Where(x => x.RefId == groupGuid.Value.ToString());

		if (categories != null && categories.Length > 0)
			query = query.Where(x => categories.Contains(x.Category));

		if (types != null && types.Length > 0)
			query = query.Where(x => types.Contains(x.Type));

		if (authorGuid != null)
			query = query.Where(x => x.Author != null && x.Author.Guid == authorGuid.Value);

		query = query
			.OrderByDescending(x => x.Id);

		if (take.HasValue)
			query = query.Take(take.Value);

		return await query
			.ToArrayAsync();
	}

	/// <summary>
	/// Создание новой записи в журнале аудита
	/// </summary>
	/// <param name="text">Сообщение</param>
	/// <param name="details">Детали</param>
	/// <param name="referenceId">Идентификатор связанного объекта</param>
	/// <param name="category">Категория</param>
	/// <param name="type">Тип</param>
	/// <param name="user">идентификатор пользователя, чьё действие вызвало запись сообщения</param>
	public async Task WriteLog(
		string text,
		string? details = null,
		string? referenceId = null,
		LogCategory category = LogCategory.Core,
		LogType type = LogType.Trace,
		Guid? user = null)
	{
		await db.InsertAsync(new Log
		{
			Category = category,
			Date = DateFormats.GetCurrentDateTime(),
			Type = type,
			UserGuid = user,
			Text = text,
			Details = details,
			RefId = referenceId,
		});
	}

	/// <summary>
	/// Получение настроек приложения
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Настройки</returns>
	public async Task<SettingsInfo> GetSettingsAsync(UserAuthInfo user)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return await GetSettingsAsync();
	}

	/// <summary>
	/// Изменение настроек приложения
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="newSettings">Новые настройки</param>
	public async Task UpdateSettingsAsync(UserAuthInfo user, SettingsInfo newSettings)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);
		User = user.Guid;

		await UpdateSettingsAsync(newSettings);
	}

	/// <summary>
	/// Перестроение кэша системы получения данных
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	public async Task RebuildStorageCacheAsync(UserAuthInfo user)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);
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

	Guid? User { get; set; } = null;

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
				Type = t.Type,
				SourceType = s.Type,
				Frequency = t.Frequency,
				ScalingCoefficient = t.IsScaling
					? ((t.MaxEu - t.MinEu) / (t.MaxRaw - t.MinRaw))
					: 1,
			}
		).ToDictionaryAsync(x => x.Id, x => x);

		// создание таблицы для значений на текущую дату
		if (!TablesRepository.CachedTables.ContainsKey(DateTime.Today))
		{
			await db.TablesRepository.GetHistoryTableAsync(DateTime.Today);
		}

		// актуализация таблицы текущих значений
		await db.ValuesRepository.CreateLiveValues();
	}

	static object locker = new();

	#endregion

	#region Запросы

	/// <summary>
	/// Запрос сообщений аудита
	/// </summary>
	public IQueryable<LogInfo> QueryLogs()
	{
		var query =
			from log in db.Logs
			from user in db.Users.LeftJoin(x => x.Guid == log.UserGuid)
			select new LogInfo
			{
				Id = log.Id,
				Category = log.Category,
				DateString = log.Date.ToString(DateFormats.Standart),
				Text = log.Text,
				Type = log.Type,
				RefId = log.RefId,
				Author = user == null ? null : new UserSimpleInfo
				{
					Guid = user.Guid,
					FullName = user.FullName ?? user.Login ?? string.Empty,
				}
			};

		return query;
	}

	#endregion
}
