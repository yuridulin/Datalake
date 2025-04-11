using Datalake.Database.Extensions;
using Datalake.Database.Tables;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.LogModels;
using Datalake.PublicApi.Models.Settings;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.UserGroups;
using Datalake.PublicApi.Models.Users;
using LinqToDB;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с настройками и кэшами
/// </summary>
public static class SystemRepository
{
	#region Действия

	/// <summary>
	/// Получение списка сообщений
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="lastId">Идентификатор сообщения, с которого начать отсчёт количества в сторону более поздних</param>
	/// <param name="firstId">Идентификатор сообщения, с которого начать отсчёт количества в сторону более ранних</param>
	/// <param name="take">Сколько сообщений получить за этот запрос</param>
	/// <param name="sourceId">Идентификатор затронутого источника</param>
	/// <param name="blockId">Идентификатор затронутого блока</param>
	/// <param name="tagGuid">Идентификатор затронутого тега</param>
	/// <param name="userGuid">Идентификатор затронутого пользователя</param>
	/// <param name="groupGuid">Идентификатор затронутой группы пользователей</param>
	/// <param name="categories">Выбранные категории сообщений</param>
	/// <param name="types">Выбранные типы сообщений</param>
	/// <param name="authorGuid">Идентификатор пользователя, создавшего сообщение</param>
	/// <returns>Список сообщений</returns>
	public static async Task<LogInfo[]> GetLogsAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int? lastId = null,
		int? firstId = null,
		int? take = null,
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

		var query = QueryLogs(db,
			includeDeletedObjects: false,
			authorGuid,
			sourceId,
			blockId,
			tagGuid,
			userGuid,
			groupGuid);

		if (categories != null && categories.Length > 0)
			query = query.Where(x => categories.Contains(x.Category));

		if (types != null && types.Length > 0)
			query = query.Where(x => types.Contains(x.Type));

		if (authorGuid != null)
			query = query.Where(x => x.Author != null && x.Author.Guid == authorGuid.Value);

		query = query
			.OrderByDescending(x => x.Id);

		if (lastId.HasValue)
			query = query.Where(x => x.Id > lastId.Value);
		else if (firstId.HasValue)
			query = query.Where(x => x.Id < firstId.Value);

		if (take.HasValue)
			query = query.Take(take.Value);

		return await query
			.ToArrayAsync();
	}

	/// <summary>
	/// Создание новой записи в журнале аудита
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="text">Сообщение</param>
	/// <param name="details">Детали</param>
	/// <param name="referenceId">Идентификатор связанного объекта</param>
	/// <param name="category">Категория</param>
	/// <param name="type">Тип</param>
	/// <param name="user">идентификатор пользователя, чьё действие вызвало запись сообщения</param>
	public static async Task WriteLog(
		DatalakeContext db,
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
			AuthorGuid = user,
			Text = text,
			Details = details,
			RefId = referenceId,
		});
	}

	/// <summary>
	/// Получение настроек приложения
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Настройки</returns>
	public static async Task<SettingsInfo> GetSettingsAsync(
		DatalakeContext db, UserAuthInfo user)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		return await GetSettingsAsync(db);
	}

	/// <summary>
	/// Изменение настроек приложения
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="newSettings">Новые настройки</param>
	public static async Task UpdateSettingsAsync(
		DatalakeContext db, UserAuthInfo user, SettingsInfo newSettings)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		await UpdateSettingsAsync(db, user.Guid, newSettings);
	}

	/// <summary>
	/// Перестроение кэша системы получения данных
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	public static async Task RebuildStorageCacheAsync(
		DatalakeContext db, UserAuthInfo user)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);

		await RebuildStorageCacheAsync(db);

		await db.InsertAsync(new Log
		{
			Category = LogCategory.Core,
			Type = LogType.Success,
			Text = "Перезапуск служб получения данных",
			AuthorGuid = user.Guid,
		});
	}

	/// <summary>
	/// Получение настроек приложения от имени приложения
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <returns>Настройки</returns>
	public static async Task<SettingsInfo> GetSettingsAsSystemAsync(DatalakeContext db)
	{
		return await GetSettingsAsync(db);
	}

	#endregion

	#region Реализация

	internal static async Task<SettingsInfo> GetSettingsAsync(DatalakeContext db)
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

	internal static async Task UpdateSettingsAsync(
		DatalakeContext db, Guid userGuid, SettingsInfo newSettings)
	{
		try
		{
			var settings = await GetSettingsAsync(db);

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
				AuthorGuid = userGuid,
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

	internal static async Task RebuildStorageCacheAsync(DatalakeContext db)
	{
		var tables = await TablesRepository.GetHistoryTablesFromSchema(db);

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
				SourceId = t.SourceId,
				SourceType = s.Type,
				Frequency = t.Frequency,
				ScalingCoefficient = t.IsScaling
					? ((t.MaxEu - t.MinEu) / (t.MaxRaw - t.MinRaw))
					: 1,
				IsDeleted = t.IsDeleted,
			}
		).ToDictionaryAsync(x => x.Id, x => x);

		// создание таблицы для значений на текущую дату
		if (!TablesRepository.CachedTables.ContainsKey(DateTime.Today))
		{
			await TablesRepository.GetHistoryTableAsync(db, DateTime.Today);
		}

		// актуализация таблицы текущих значений
		await ValuesRepository.CreateLiveValues(db);
	}

	static object locker = new();

	#endregion

	#region Запросы

	/// <summary>
	/// Запрос сообщений аудита
	/// </summary>
	public static IQueryable<LogInfo> QueryLogs(
		DatalakeContext db,
		bool includeDeletedObjects = false,
		Guid? authorGuid = null,
		int? sourceId = null,
		int? blockId = null,
		Guid? tagGuid = null,
		Guid? userGuid = null,
		Guid? userGroupGuid = null)
	{
		var query =
			from log in db.Logs
			from author in db.Users.LeftJoin(x => x.Guid == log.AuthorGuid && (includeDeletedObjects || !x.IsDeleted))
			from source in db.Sources.LeftJoin(x => x.Id == log.AffectedSourceId && (includeDeletedObjects || !x.IsDeleted))
			from block in db.Blocks.LeftJoin(x => x.Id == log.AffectedBlockId && (includeDeletedObjects || !x.IsDeleted))
			from tag in db.Tags.LeftJoin(x => x.Id == log.AffectedTagId && (includeDeletedObjects || !x.IsDeleted))
			from user in db.Users.LeftJoin(x => x.Guid == log.AffectedUserGuid && (includeDeletedObjects || !x.IsDeleted))
			from userGroup in db.UserGroups.LeftJoin(x => x.Guid == log.AffectedUserGroupGuid && (includeDeletedObjects || !x.IsDeleted))
			from tagSource in db.Sources.LeftJoin(x => x.Id == tag.SourceId && (includeDeletedObjects || !x.IsDeleted))
			where
				(authorGuid == null || author.Guid == authorGuid.Value) &&
				(sourceId == null || source.Id == sourceId.Value) &&
				(blockId == null || block.Id == blockId.Value) &&
				(tagGuid == null || tag.GlobalGuid == tagGuid.Value) &&
				(userGuid == null || user.Guid == userGuid.Value) &&
				(userGroupGuid == null || userGroup.Guid == userGroupGuid.Value)
			select new LogInfo
			{
				Id = log.Id,
				Category = log.Category,
				DateString = log.Date.ToString(DateFormats.Standart),
				Text = log.Text,
				Type = log.Type,
				Details = log.Details,
				Author = author == null ? null : new UserSimpleInfo
				{
					Guid = author.Guid,
					FullName = author.FullName ?? author.Login ?? string.Empty,
				},
				AffectedSource = source == null ? null : new SourceSimpleInfo
				{
					Id = source.Id,
					Name = source.Name,
				},
				AffectedBlock = block == null ? null : new BlockSimpleInfo
				{
					Id = block.Id,
					Guid = block.GlobalId,
					Name = block.Name,
				},
				AffectedTag = tag == null ? null : new TagSimpleInfo
				{
					Id = tag.Id,
					Guid = tag.GlobalGuid,
					Name = tag.Name,
					Type = tag.Type,
					Frequency = tag.Frequency,
					SourceType = tagSource == null ? SourceType.NotSet : tagSource.Type,
				},
				AffectedUser = user == null ? null : new UserSimpleInfo
				{
					Guid = user.Guid,
					FullName = user.FullName ?? user.Login ?? string.Empty,
				},
				AffectedUserGroup = userGroup == null ? null : new UserGroupSimpleInfo
				{
					Guid = userGroup.Guid,
					Name = userGroup.Name,
				},
			};

		return query;
	}

	#endregion
}
