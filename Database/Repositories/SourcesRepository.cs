using Datalake.Database.Extensions;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Sources;
using LinqToDB;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с источниками данных
/// </summary>
public static class SourcesRepository
{
	#region Действия

	/// <summary>
	/// Создание нового источника
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceInfo">Параметры нового источника</param>
	/// <returns>Идентификатор нового источника</returns>
	public static async Task<SourceInfo> CreateAsync(
		DatalakeContext db,
		UserAuthInfo user,
		SourceInfo? sourceInfo = null)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Manager);

		if (sourceInfo != null)
			return await CreateAsync(db, user.Guid, sourceInfo);

		return await CreateAsync(db, user.Guid);
	}

	/// <summary>
	/// Получение информации об источнике
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Информация об источнике</returns>
	public static async Task<SourceInfo> ReadAsync(DatalakeContext db, UserAuthInfo user, int id)
	{
		AccessRepository.ThrowIfNoAccessToSource(user, AccessType.Viewer, id);

		var source = await QueryInfo(db).Where(x => x.Id == id).FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "источник #" + id);

		source.AccessRule = user.Sources[id];

		return source;
	}

	/// <summary>
	/// Получение информации об источнике, включая теги, зависящие от него
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Информация об источнике</returns>
	public static async Task<SourceWithTagsInfo> ReadWithTagsAsync(DatalakeContext db, UserAuthInfo user, int id)
	{
		AccessRepository.ThrowIfNoAccessToSource(user, AccessType.Viewer, id);

		var source = await QueryInfoWithTags(db).Where(x => x.Id == id).FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "источник #" + id);

		source.AccessRule = user.Sources[id];

		foreach (var tag in source.Tags)
		{
			var rule = user.Tags.TryGetValue(tag.Guid, out var r) ? r : AccessRuleInfo.Default;
			tag.AccessRule = rule;

			if (!rule.AccessType.HasAccess(AccessType.Viewer))
			{
				tag.Guid = Guid.Empty;
				tag.Name = string.Empty;
				tag.Frequency = TagFrequency.NotSet;
			}
		}

		return source;
	}

	/// <summary>
	/// Получение списка источников
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="withCustom">Включать в список системные источники</param>
	/// <returns>Список источников</returns>
	public static async Task<SourceInfo[]> ReadAllAsync(DatalakeContext db, UserAuthInfo user, bool withCustom)
	{
		var sources = await QueryInfo(db, withCustom).ToArrayAsync();

		foreach (var source in sources)
		{
			var rule = user.Sources.TryGetValue(source.Id, out var r) ? r : AccessRuleInfo.Default;
			source.AccessRule = rule;
		}

		return sources.Where(x => x.AccessRule.AccessType.HasAccess(AccessType.Viewer)).ToArray();
	}

	/// <summary>
	/// Изменение параметров источника
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <param name="sourceInfo">Новые параметры источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public static async Task<bool> UpdateAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int id,
		SourceInfo sourceInfo)
	{
		AccessRepository.ThrowIfNoAccessToSource(user, AccessType.Editor, id);

		return await UpdateAsync(db, user.Guid, id, sourceInfo);
	}

	/// <summary>
	/// Удаление источника
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public static async Task<bool> DeleteAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int id)
	{
		AccessRepository.ThrowIfNoAccessToSource(user, AccessType.Manager, id);

		return await DeleteAsync(db, user.Guid, id);
	}

	#endregion

	#region Реализация

	internal static async Task<SourceInfo> CreateAsync(DatalakeContext db, Guid userGuid)
	{
		var transaction = await db.BeginTransactionAsync();

		int? id = await db.Sources
			.Value(x => x.Name, "INSERTING")
			.Value(x => x.Address, "")
			.Value(x => x.Type, SourceType.NotSet)
			.InsertWithInt32IdentityAsync();

		string name = ValueChecker.RemoveWhitespaces("Новый источник #" + id.Value, "_");

		await db.Sources
			.Where(x => x.Id == id.Value)
			.Set(x => x.Name, name)
			.UpdateAsync();

		await LogAsync(db, userGuid, id.Value, "Создан источник: " + name);

		await transaction.CommitAsync();

		SystemRepository.Update();
		AccessRepository.Update();

		var info = await QueryInfo(db).FirstOrDefaultAsync(x => x.Id == id.Value)
			?? throw new NotFoundException($"Источник #{id} не найден");

		return info;
	}

	internal static async Task<SourceInfo> CreateAsync(DatalakeContext db, Guid userGuid, SourceInfo sourceInfo)
	{
		sourceInfo.Name = ValueChecker.RemoveWhitespaces(sourceInfo.Name, "_");

		if (await SourcesNotDeleted(db).AnyAsync(x => x.Name == sourceInfo.Name))
			throw new AlreadyExistException("Уже существует источник с таким именем");

		if (sourceInfo.Type == SourceType.System)
			throw new InvalidValueException("Нельзя добавить системный источник");

		var transaction = await db.BeginTransactionAsync();

		int? id = await db.Sources
			.Value(x => x.Name, sourceInfo.Name)
			.Value(x => x.Description, sourceInfo.Description)
			.Value(x => x.Address, sourceInfo.Address)
			.Value(x => x.Type, sourceInfo.Type)
			.InsertWithInt32IdentityAsync();

		await LogAsync(db, userGuid, id.Value, "Создан источник: " + sourceInfo.Name);

		await transaction.CommitAsync();

		SystemRepository.Update();
		AccessRepository.Update();

		var info = await QueryInfo(db).FirstOrDefaultAsync(x => x.Id == id.Value)
			?? throw new NotFoundException($"Источник #{id} не найден");

		return info;
	}

	internal static async Task<bool> UpdateAsync(DatalakeContext db, Guid userGuid, int id, SourceInfo sourceInfo)
	{
		sourceInfo.Name = ValueChecker.RemoveWhitespaces(sourceInfo.Name, "_");

		var source = await SourcesNotDeleted(db)
			.Where(x => x.Id == id)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Источник #{id} не найден");

		if (await SourcesNotDeleted(db).AnyAsync(x => x.Name == sourceInfo.Name && x.Id != id))
			throw new AlreadyExistException("Уже существует источник с таким именем");

		var transaction = await db.BeginTransactionAsync();

		int count = await db.Sources
			.Where(x => x.Id == id)
			.Set(x => x.Name, sourceInfo.Name)
			.Set(x => x.Description, sourceInfo.Description)
			.Set(x => x.Address, sourceInfo.Address)
			.Set(x => x.Type, sourceInfo.Type)
			.UpdateAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось обновить источник #{id}", DatabaseStandartError.UpdatedZero);

		await LogAsync(db, userGuid, id, "Изменен источник: " + sourceInfo.Name, ObjectExtension.Difference(
			new { source.Name, source.Address, source.Type },
			new { sourceInfo.Name, sourceInfo.Address, sourceInfo.Type }));

		await transaction.CommitAsync();

		SystemRepository.Update();

		return true;
	}

	internal static async Task<bool> DeleteAsync(DatalakeContext db, Guid userGuid, int id)
	{
		using var transaction = await db.BeginTransactionAsync();

		var name = await SourcesNotDeleted(db)
			.Where(x => x.Id == id)
			.Select(x => x.Name)
			.FirstOrDefaultAsync();

		var count = await db.Sources
			.Where(x => x.Id == id)
			.Set(x => x.IsDeleted, true)
			.UpdateAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось удалить источник #{id}", DatabaseStandartError.DeletedZero);

		await LogAsync(db, userGuid, id, "Удален источник: " + name + ".");

		await transaction.CommitAsync();

		SystemRepository.Update();
		AccessRepository.Update();

		return true;
	}

	internal static async Task LogAsync(DatalakeContext db, Guid userGuid, int id, string message, string? details = null)
	{
		await db.InsertAsync(new Log
		{
			Category = LogCategory.Source,
			RefId = id.ToString(),
			AffectedSourceId = id,
			AuthorGuid = userGuid,
			Text = message,
			Type = LogType.Success,
			Details = details,
		});
	}

	#endregion

	#region Запросы

	/// <summary>
	/// Не настраиваемые источники данных
	/// </summary>
	internal static readonly SourceType[] CustomSourcesId = [SourceType.System, SourceType.Calculated, SourceType.Manual, SourceType.Aggregated, SourceType.NotSet];

	internal static IQueryable<Source> SourcesNotDeleted(DatalakeContext db)
	{
		return db.Sources.Where(x => !x.IsDeleted);
	}

	/// <summary>
	/// Запрос информации о источниках без связей
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="withCustom">Включать ли системные источники в запрос</param>
	public static IQueryable<SourceInfo> QueryInfo(DatalakeContext db, bool withCustom = false)
	{
		var query =
			from source in db.Sources
			where !source.IsDeleted && (withCustom || !CustomSourcesId.Cast<int>().Contains(source.Id))
			select new SourceInfo
			{
				Id = source.Id,
				Name = source.Name,
				Address = source.Address,
				Description = source.Description,
				Type = source.Type,
			};

		return query;
	}

	/// <summary>
	/// Запрос информации о источниках вместе со списками зависящих тегов
	/// </summary>
	public static IQueryable<SourceWithTagsInfo> QueryInfoWithTags(DatalakeContext db)
	{
		var query =
			from source in db.Sources
			where !source.IsDeleted
			select new SourceWithTagsInfo
			{
				Id = source.Id,
				Address = source.Address,
				Name = source.Name,
				Type = source.Type,
				Tags = (
					from tag in db.Tags.LeftJoin(x => x.SourceId == source.Id && !x.IsDeleted)
					select new SourceTagInfo
					{
						Id = tag.Id,
						Guid = tag.GlobalGuid,
						Item = tag.SourceItem ?? string.Empty,
						FormulaInputs = Array.Empty<SourceTagInfo.TagInputMinimalInfo>(),
						Name = tag.Name,
						Type = tag.Type,
						Frequency = tag.Frequency,
						SourceType = source.Type,
						Aggregation = tag.Aggregation,
						AggregationPeriod = tag.AggregationPeriod,
					}
				).ToArray(),
			};

		return query;
	}
	
	/// <summary>
	/// Запрос информации о источниках вместе со списками зависящих тегов
	/// </summary>
	public static IQueryable<SourceWithTagsInfo> QueryInfoWithTagsAndSourceTags(DatalakeContext db)
	{
		var query =
			from source in db.Sources
			where !source.IsDeleted
			select new SourceWithTagsInfo
			{
				Id = source.Id,
				Address = source.Address,
				Name = source.Name,
				Type = source.Type,
				Tags = (
					from tag in db.Tags.Where(x => !x.IsDeleted)
					from sourceTag in db.Tags.LeftJoin(x => x.Id == tag.SourceTagId && !x.IsDeleted)
					where tag.SourceId == source.Id
					select new SourceTagInfo
					{
						Id = tag.Id,
						Guid = tag.GlobalGuid,
						Item = tag.SourceItem ?? string.Empty,
						Formula = tag.Formula,
						FormulaInputs = (
							from rel in db.TagInputs
							from input in db.Tags.LeftJoin(x => x.Id == rel.InputTagId)
							where rel.TagId == tag.Id && input != null
							select new SourceTagInfo.TagInputMinimalInfo
							{
								InputTagId = input.Id,
								InputTagGuid = input.GlobalGuid,
								VariableName = rel.VariableName,
							}
						).ToArray(),
						Name = tag.Name,
						Type = tag.Type,
						Frequency = tag.Frequency,
						SourceType = source.Type,
						Aggregation = tag.Aggregation,
						AggregationPeriod = tag.AggregationPeriod,
						SourceTag = sourceTag == null ? null : new SourceTagInfo.TagInputMinimalInfo
						{
							InputTagId = sourceTag.Id,
							InputTagGuid = sourceTag.GlobalGuid,
							VariableName = sourceTag.Name
						},
					}
				).ToArray(),
			};

		return query;
	}

	/// <summary>
	/// Запрос информации о зависящих от источника тегов по его идентификатору
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="id">Идентификатор источника</param>
	internal static IQueryable<SourceTagInfo> QueryExistTags(DatalakeContext db, int id)
	{
		var query =
			from source in db.Sources.Where(x => x.Id == id && !x.IsDeleted)
			from tag in db.Tags.InnerJoin(x => x.SourceId == source.Id && !x.IsDeleted)
			select new SourceTagInfo
			{
				Id = tag.Id,
				Guid = tag.GlobalGuid,
				Name = tag.Name,
				Type = tag.Type,
				Item = tag.SourceItem ?? string.Empty,
				Frequency = tag.Frequency,
				SourceType = source.Type,
				Formula = tag.Formula,
				FormulaInputs = (
					from rel in db.TagInputs
					from input in db.Tags.LeftJoin(x => x.Id == rel.InputTagId)
					where rel.TagId == tag.Id && input != null
					select new SourceTagInfo.TagInputMinimalInfo
					{
						InputTagId = input.Id,
						InputTagGuid = input.GlobalGuid,
						VariableName = rel.VariableName,
					}
				).ToArray(),
			};

		return query;
	}

	#endregion
}
