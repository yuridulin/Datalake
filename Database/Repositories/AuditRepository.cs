using Datalake.Database.Functions;
using Datalake.Database.Tables;
using Datalake.PublicApi.Constants;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.LogModels;
using Datalake.PublicApi.Models.Sources;
using Datalake.PublicApi.Models.Tags;
using Datalake.PublicApi.Models.UserGroups;
using Datalake.PublicApi.Models.Users;
using LinqToDB;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с сообщениями аудита
/// </summary>
public class AuditRepository
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
	public static async Task<LogInfo[]> ReadAsync(
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
		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Editor);

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
	public static async Task WriteAsync(
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

	#endregion

	private static IQueryable<LogInfo> QueryLogs(
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
}
