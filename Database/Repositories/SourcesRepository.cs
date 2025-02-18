using Datalake.Database.Enums;
using Datalake.Database.Exceptions;
using Datalake.Database.Extensions;
using Datalake.Database.Models.Auth;
using Datalake.Database.Models.Sources;
using Datalake.Database.Tables;
using LinqToDB;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с источниками данных
/// </summary>
public class SourcesRepository(DatalakeContext db)
{
	#region Действия

	/// <summary>
	/// Создание нового источника
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceInfo">Параметры нового источника</param>
	/// <returns>Идентификатор нового источника</returns>
	public async Task<int> CreateAsync(
		UserAuthInfo user,
		SourceInfo? sourceInfo = null)
	{
		AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);
		User = user.Guid;

		if (sourceInfo != null)
			return await CreateAsync(sourceInfo);

		return await CreateAsync();
	}

	/// <summary>
	/// Получение информации об источнике
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Информация об источнике</returns>
	public async Task<SourceInfo> ReadAsync(UserAuthInfo user, int id)
	{
		AccessRepository.ThrowIfNoAccessToSource(user, AccessType.Viewer, id);

		var source = await QueryInfo().Where(x => x.Id == id).FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "источник #" + id);

		source.AccessRule = user.Sources[id];

		return source;
	}

	/// <summary>
	/// Получение информации об источнике, включая теги, зависящие от него
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Информация об источнике</returns>
	public async Task<SourceWithTagsInfo> ReadWithTagsAsync(UserAuthInfo user, int id)
	{
		AccessRepository.ThrowIfNoAccessToSource(user, AccessType.Viewer, id);

		var source = await QueryInfoWithTags().Where(x => x.Id == id).FirstOrDefaultAsync()
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
	/// <param name="user">Информация о пользователе</param>
	/// <param name="withCustom">Включать в список системные источники</param>
	/// <returns>Список источников</returns>
	public async Task<SourceInfo[]> ReadAllAsync(UserAuthInfo user, bool withCustom)
	{
		var sources = await QueryInfo(withCustom).ToArrayAsync();

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
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <param name="sourceInfo">Новые параметры источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> UpdateAsync(
		UserAuthInfo user,
		int id,
		SourceInfo sourceInfo)
	{
		AccessRepository.ThrowIfNoAccessToSource(user, AccessType.Admin, id);
		User = user.Guid;

		return await UpdateAsync(id, sourceInfo);
	}

	/// <summary>
	/// Удаление источника
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(
		UserAuthInfo user,
		int id)
	{
		AccessRepository.ThrowIfNoAccessToSource(user, AccessType.Admin, id);
		User = user.Guid;

		return await DeleteAsync(id);
	}

	#endregion

	#region Реализация

	Guid? User { get; set; } = null;

	internal async Task<int> CreateAsync()
	{
		var transaction = await db.BeginTransactionAsync();

		int? id = await db.Sources
			.Value(x => x.Name, "INSERTING")
			.Value(x => x.Address, "")
			.Value(x => x.Type, SourceType.Unknown)
			.InsertWithInt32IdentityAsync();

		string name = ValueChecker.RemoveWhitespaces("Новый источник #" + id.Value, "_");

		await db.Sources
			.Where(x => x.Id == id.Value)
			.Set(x => x.Name, name)
			.UpdateAsync();

		await LogAsync(id.Value, "Создан источник: " + name);

		await transaction.CommitAsync();

		SystemRepository.Update();
		AccessRepository.Update();

		return id.Value;
	}

	internal async Task<int> CreateAsync(SourceInfo sourceInfo)
	{
		sourceInfo.Name = ValueChecker.RemoveWhitespaces(sourceInfo.Name, "_");

		if (await db.Sources.AnyAsync(x => x.Name == sourceInfo.Name))
			throw new AlreadyExistException("Уже существует источник с таким именем");

		if (sourceInfo.Type == SourceType.Custom)
			throw new InvalidValueException("Нельзя добавить системный источник");

		var transaction = await db.BeginTransactionAsync();

		int? id = await db.Sources
			.Value(x => x.Name, sourceInfo.Name)
			.Value(x => x.Address, sourceInfo.Address)
			.Value(x => x.Type, sourceInfo.Type)
			.InsertWithInt32IdentityAsync();

		await LogAsync(id.Value, "Создан источник: " + sourceInfo.Name);

		await transaction.CommitAsync();

		SystemRepository.Update();
		AccessRepository.Update();

		return id.Value;
	}

	internal async Task<bool> UpdateAsync(int id, SourceInfo sourceInfo)
	{
		sourceInfo.Name = ValueChecker.RemoveWhitespaces(sourceInfo.Name, "_");

		var source = await db.Sources
			.Where(x => x.Id == id)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Источник #{id} не найден");

		if (await db.Sources.AnyAsync(x => x.Name == sourceInfo.Name && x.Id != id))
			throw new AlreadyExistException("Уже существует источник с таким именем");

		var transaction = await db.BeginTransactionAsync();

		int count = await db.Sources
			.Where(x => x.Id == id)
			.Set(x => x.Name, sourceInfo.Name)
			.Set(x => x.Address, sourceInfo.Address)
			.Set(x => x.Type, sourceInfo.Type)
			.UpdateAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось обновить источник #{id}", DatabaseStandartError.UpdatedZero);

		await LogAsync(id, "Изменен источник: " + sourceInfo.Name, ObjectExtension.Difference(
			new { source.Name, source.Address, source.Type },
			new { sourceInfo.Name, sourceInfo.Address, sourceInfo.Type }));

		await transaction.CommitAsync();

		SystemRepository.Update();

		return true;
	}

	internal async Task<bool> DeleteAsync(int id)
	{
		using var transaction = await db.BeginTransactionAsync();

		var name = await db.Sources
			.Where(x => x.Id == id)
			.Select(x => x.Name)
			.FirstOrDefaultAsync();

		var count = await db.Sources
			.Where(x => x.Id == id)
			.DeleteAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось удалить источник #{id}", DatabaseStandartError.DeletedZero);

		// при удалении источника его теги становятся ручными
		int tagsCount = await db.Tags
			.Where(x => x.SourceId == id)
			.Set(x => x.SourceId, (int)CustomSource.Manual)
			.UpdateAsync();

		await LogAsync(id, "Удален источник: " + name + ". Затронуто тегов: " + tagsCount);

		await transaction.CommitAsync();

		SystemRepository.Update();
		AccessRepository.Update();

		return true;
	}

	internal async Task LogAsync(int id, string message, string? details = null)
	{
		await db.InsertAsync(new Log
		{
			Category = LogCategory.Source,
			RefId = id.ToString(),
			UserGuid = User,
			Text = message,
			Type = LogType.Success,
			Details = details,
		});
	}

	#endregion

	#region Запросы

	static int[] CustomSourcesId = Enum.GetValues<CustomSource>().Cast<int>().ToArray();

	/// <summary>
	/// Запрос информации о источниках без связей
	/// </summary>
	/// <param name="withCustom">Включать ли системные источники в запрос</param>
	public IQueryable<SourceInfo> QueryInfo(bool withCustom = false)
	{
		var query =
			from source in db.Sources
			where withCustom || !CustomSourcesId.Contains(source.Id)
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
	public IQueryable<SourceWithTagsInfo> QueryInfoWithTags()
	{
		var query =
			from source in db.Sources
			select new SourceWithTagsInfo
			{
				Id = source.Id,
				Address = source.Address,
				Name = source.Name,
				Type = source.Type,
				Tags = (
					from tag in db.Tags
					where tag.SourceId == source.Id
					select new SourceTagInfo
					{
						Guid = tag.GlobalGuid,
						Item = tag.SourceItem ?? string.Empty,
						Name = tag.Name,
						Type = tag.Type,
						Frequency = tag.Frequency,
					}
				).ToArray(),
			};

		return query;
	}

	/// <summary>
	/// Запрос информации о зависящих от источника тегов по его идентификатору
	/// </summary>
	/// <param name="id">Идентификатор источника</param>
	internal IQueryable<SourceTagInfo> QueryExistTags(int id)
	{
		var query = db.Tags
			.Where(x => x.SourceId == id)
			.Select(x => new SourceTagInfo
			{
				Guid = x.GlobalGuid,
				Name = x.Name,
				Type = x.Type,
				Item = x.SourceItem ?? string.Empty,
				Frequency = x.Frequency,
			});

		return query;
	}

	#endregion
}
