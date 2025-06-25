using Datalake.Database.Extensions;
using Datalake.Database.Functions;
using Datalake.Database.InMemory.Models;
using Datalake.Database.InMemory.Queries;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Sources;
using LinqToDB;

namespace Datalake.Database.InMemory.Repositories;

/// <summary>
/// Репозиторий источников данных
/// </summary>
public class SourcesMemoryRepository(DatalakeDataStore dataStore)
{
	#region Действия

	/// <summary>
	/// Создание нового источника
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceInfo">Параметры нового источника</param>
	/// <returns>Идентификатор нового источника</returns>
	public async Task<SourceInfo> CreateAsync(
		DatalakeContext db,
		UserAuthInfo user,
		SourceInfo? sourceInfo = null)
	{
		AccessChecks.ThrowIfNoGlobalAccess(user, AccessType.Manager);

		if (sourceInfo != null)
			return await ProtectedCreateAsync(db, user.Guid, sourceInfo);

		return await ProtectedCreateAsync(db, user.Guid);
	}

	/// <summary>
	/// Получение информации об источнике
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Информация об источнике</returns>
	public SourceInfo Read(UserAuthInfo user, int id)
	{
		AccessChecks.ThrowIfNoAccessToSource(user, AccessType.Viewer, id);

		var source = dataStore.State.SourcesInfo().FirstOrDefault(x => x.Id == id)
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
	public SourceWithTagsInfo ReadWithTags(UserAuthInfo user, int id)
	{
		AccessChecks.ThrowIfNoAccessToSource(user, AccessType.Viewer, id);

		var source = dataStore.State.SourcesInfoWithTags().FirstOrDefault(x => x.Id == id)
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
	public SourceInfo[] ReadAll(UserAuthInfo user, bool withCustom)
	{
		var sources = dataStore.State.SourcesInfo(withCustom).ToArray();

		List<SourceInfo> sourcesWithAccess = [];
		foreach (var source in sources)
		{
			source.AccessRule = user.Sources.TryGetValue(source.Id, out var r) ? r : AccessRuleInfo.Default;
			if (source.AccessRule.AccessType.HasAccess(AccessType.Viewer))
				sourcesWithAccess.Add(source);
		}

		return sourcesWithAccess.ToArray();
	}

	/// <summary>
	/// Изменение параметров источника
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <param name="sourceInfo">Новые параметры источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> UpdateAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int id,
		SourceInfo sourceInfo)
	{
		AccessChecks.ThrowIfNoAccessToSource(user, AccessType.Editor, id);

		return await ProtectedUpdateAsync(db, user.Guid, id, sourceInfo);
	}

	/// <summary>
	/// Удаление источника
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int id)
	{
		AccessChecks.ThrowIfNoAccessToSource(user, AccessType.Manager, id);

		return await ProtectedDeleteAsync(db, user.Guid, id);
	}

	#endregion

	internal async Task<SourceInfo> ProtectedCreateAsync(DatalakeContext db, Guid userGuid)
	{
		// Проверки, не требующие стейта
		Source newSource = new()
		{
			Name = "INSERTING",
			Description = string.Empty,
			Address = "",
			Type = SourceType.Inopc,
			IsDeleted = false,
		};

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				int id = await db.Sources
					.Value(x => x.Name, newSource.Name)
					.Value(x => x.Description, newSource.Description)
					.Value(x => x.Address, newSource.Address)
					.Value(x => x.Type, newSource.Type)
					.InsertWithInt32IdentityAsync()
					?? throw new Exception("Не получен id из БД");

				newSource.Id = id;
				newSource.Name = StringExtensions.RemoveWhitespaces("Новый источник #" + id, "_");

				await db.Sources
					.Where(x => x.Id == id)
					.Set(x => x.Name, newSource.Name)
					.UpdateAsync();

				await LogAsync(db, userGuid, id, "Создан источник: " + newSource.Name);

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
				Sources = state.Sources.Add(newSource),
			});
		}

		// Возвращение ответа
		var info = new SourceInfo
		{
			Id = newSource.Id,
			Name = newSource.Name,
			Address = newSource.Address,
			Type = newSource.Type,
			Description = newSource.Description,
		};

		return info;
	}

	internal async Task<SourceInfo> ProtectedCreateAsync(DatalakeContext db, Guid userGuid, SourceInfo sourceInfo)
	{
		// Проверки, не требующие стейта
		sourceInfo.Name = StringExtensions.RemoveWhitespaces(sourceInfo.Name, "_");

		if (sourceInfo.Type == SourceType.System)
			throw new InvalidValueException("Нельзя добавить системный источник");

		Source newSource = new()
		{
			Name = sourceInfo.Name,
			Address = sourceInfo.Address,
			Type = sourceInfo.Type,
			Description = sourceInfo.Description,
			IsDeleted = false,
		};

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (currentState.Sources.Any(x => !x.IsDeleted && x.Name == sourceInfo.Name))
				throw new AlreadyExistException("Уже существует источник с таким именем");

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				int id = await db.Sources
					.Value(x => x.Name, newSource.Name)
					.Value(x => x.Description, newSource.Description)
					.Value(x => x.Address, newSource.Address)
					.Value(x => x.Type, newSource.Type)
					.InsertWithInt32IdentityAsync()
					?? throw new Exception("Не получен id из БД");

				newSource.Id = id;

				await LogAsync(db, userGuid, id, "Создан источник: " + sourceInfo.Name);

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
				Sources = state.Sources.Add(newSource),
			});
		}

		// Возвращение ответа
		var info = new SourceInfo
		{
			Id = newSource.Id,
			Name = newSource.Name,
			Address = newSource.Address,
			Type = newSource.Type,
			Description = newSource.Description,
		};

		return info;
	}

	internal async Task<bool> ProtectedUpdateAsync(DatalakeContext db, Guid userGuid, int id, SourceInfo sourceInfo)
	{
		// Проверки, не требующие стейта
		sourceInfo.Name = StringExtensions.RemoveWhitespaces(sourceInfo.Name, "_");

		Source? newSource;

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.SourcesById.TryGetValue(id, out var source))
				throw new NotFoundException($"Источник #{id} не найден");

			if (currentState.Sources.Any(x => !x.IsDeleted && x.Id != id && x.Name == sourceInfo.Name))
				throw new AlreadyExistException("Уже существует источник с таким именем");

			newSource = source with
			{
				Id = id,
				Name = sourceInfo.Name,
				Address = sourceInfo.Address,
				Type = sourceInfo.Type,
				Description = sourceInfo.Description,
				IsDeleted = false,
			};

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				int count = await db.Sources
					.Where(x => x.Id == id)
					.Set(x => x.Name, newSource.Name)
					.Set(x => x.Description, newSource.Description)
					.Set(x => x.Address, newSource.Address)
					.Set(x => x.Type, newSource.Type)
					.UpdateAsync();

				if (count == 0)
					throw new DatabaseException($"Не удалось обновить источник #{id}", DatabaseStandartError.UpdatedZero);

				await LogAsync(db, userGuid, id, "Изменен источник: " + source.Name, ObjectExtension.Difference(
					new { source.Name, source.Address, source.Type },
					new { newSource.Name, newSource.Address, newSource.Type }));

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
				Sources = state.Sources.Remove(source).Add(newSource),
			});
		}

		// Возвращение ответа
		return true;
	}

	internal async Task<bool> ProtectedDeleteAsync(DatalakeContext db, Guid userGuid, int id)
	{
		// Проверки, не требующие стейта

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.SourcesById.TryGetValue(id, out var source))
				throw new NotFoundException($"Источник #{id} не найден");

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();
			try
			{
				var count = await db.Sources
					.Where(x => x.Id == id)
					.Set(x => x.IsDeleted, true)
					.UpdateAsync();

				if (count == 0)
					throw new DatabaseException($"Не удалось удалить источник #{id}", DatabaseStandartError.DeletedZero);

				await LogAsync(db, userGuid, id, "Удален источник: " + source.Name + ".");

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
				Sources = state.Sources.Remove(source),
			});
		}

		// Возвращение ответа
		return true;
	}

	private static async Task LogAsync(DatalakeContext db, Guid userGuid, int id, string message, string? details = null)
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
}
