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
	#region API

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
		user.ThrowIfNoGlobalAccess(AccessType.Manager);

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
	public SourceInfo Get(UserAuthInfo user, int id)
	{
		var rule = user.GetAccessToSource(id);
		user.ThrowIfNoAccessToSource(AccessType.Viewer, id);

		var source = dataStore.State.SourcesInfo().FirstOrDefault(x => x.Id == id)
			?? throw new NotFoundException(message: "источник #" + id);

		source.AccessRule = rule;

		return source;
	}

	/// <summary>
	/// Получение информации об источнике, включая теги, зависящие от него
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Информация об источнике</returns>
	public SourceWithTagsInfo GetWithTags(UserAuthInfo user, int id)
	{
		user.ThrowIfNoAccessToSource(AccessType.Viewer, id);

		var source = dataStore.State.SourcesInfoWithTags().FirstOrDefault(x => x.Id == id)
			?? throw new NotFoundException(message: "источник #" + id);

		source.AccessRule = user.GetAccessToSource(id);

		foreach (var tag in source.Tags)
		{
			tag.AccessRule = user.GetAccessToTag(tag.Id);

			if (!tag.AccessRule.HasAccess(AccessType.Viewer))
			{
				tag.Guid = Guid.Empty;
				tag.Name = string.Empty;
				tag.Resolution = TagResolution.NotSet;
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
	public SourceInfo[] GetAll(UserAuthInfo user, bool withCustom)
	{
		var sources = dataStore.State.SourcesInfo(withCustom).ToArray();

		List<SourceInfo> sourcesWithAccess = [];
		foreach (var source in sources)
		{
			source.AccessRule = user.GetAccessToSource(source.Id);
			if (source.AccessRule.HasAccess(AccessType.Viewer))
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
	/// <param name="request">Новые параметры источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> UpdateAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int id,
		SourceUpdateRequest request)
	{
		user.ThrowIfNoAccessToSource(AccessType.Editor, id);

		return await ProtectedUpdateAsync(db, user.Guid, id, request);
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
		user.ThrowIfNoAccessToSource(AccessType.Manager, id);

		return await ProtectedDeleteAsync(db, user.Guid, id);
	}

	#endregion API

	#region Действия

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
			IsDisabled = false,
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
				throw new Exception("Не удалось создать источник в БД", ex);
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
			IsDisabled = newSource.IsDisabled,
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
			IsDisabled = false,
		};

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (currentState.Sources.Any(x => !x.IsDeleted && x.Name.Equals(sourceInfo.Name, StringComparison.OrdinalIgnoreCase)))
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
					.Value(x => x.IsDisabled, newSource.IsDisabled)
					.InsertWithInt32IdentityAsync()
					?? throw new Exception("Не получен id из БД");

				newSource.Id = id;

				await LogAsync(db, userGuid, id, "Создан источник: " + sourceInfo.Name);

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось обновить источник в БД", ex);
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
			IsDisabled = newSource.IsDisabled,
		};

		return info;
	}

	internal async Task<bool> ProtectedUpdateAsync(DatalakeContext db, Guid userGuid, int id, SourceUpdateRequest request)
	{
		Source? updatedSource;

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.SourcesById.TryGetValue(id, out var currentSource))
				throw new NotFoundException($"Источник #{id} не найден");

			if (currentState.Sources.Any(x => !x.IsDeleted && x.Id != id && x.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
				throw new AlreadyExistException("Уже существует источник с таким именем");

			updatedSource = currentSource with
			{
				Name = request.Name,
				Address = request.Address,
				Type = request.Type,
				Description = request.Description,
				IsDisabled = request.IsDisabled,
			};

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				int count = await db.Sources
					.Where(x => x.Id == id)
					.Set(x => x.Name, updatedSource.Name)
					.Set(x => x.Description, updatedSource.Description)
					.Set(x => x.Address, updatedSource.Address)
					.Set(x => x.Type, updatedSource.Type)
					.Set(x => x.IsDisabled, updatedSource.IsDisabled)
					.UpdateAsync();

				if (count == 0)
					throw new DatabaseException($"Не удалось обновить источник #{id}", DatabaseStandartError.UpdatedZero);

				await LogAsync(db, userGuid, id, "Изменен источник: " + currentSource.Name, ObjectExtension.Difference(
					new { currentSource.Name, currentSource.Address, currentSource.Type, currentSource.IsDisabled },
					new { updatedSource.Name, updatedSource.Address, updatedSource.Type, updatedSource.IsDisabled }));

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось обновить источник в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Sources = state.Sources.Replace(currentSource, updatedSource),
			});
		}

		// Возвращение ответа
		return true;
	}

	internal async Task<bool> ProtectedDeleteAsync(DatalakeContext db, Guid userGuid, int id)
	{
		// Проверки, не требующие стейта
		Source updatedSource;

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.SourcesById.TryGetValue(id, out var currentSource))
				throw new NotFoundException($"Источник #{id} не найден");

			updatedSource = currentSource with
			{
				IsDeleted = true,
			};

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();
			try
			{
				var count = await db.Sources
					.Where(x => x.Id == id)
					.Set(x => x.IsDeleted, updatedSource.IsDeleted)
					.UpdateAsync();

				if (count == 0)
					throw new DatabaseException($"Не удалось удалить источник #{id}", DatabaseStandartError.DeletedZero);

				await LogAsync(db, userGuid, id, "Удален источник: " + currentSource.Name + ".");

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось удалить источник из БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Sources = state.Sources.Replace(currentSource, updatedSource),
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

	#endregion Действия
}