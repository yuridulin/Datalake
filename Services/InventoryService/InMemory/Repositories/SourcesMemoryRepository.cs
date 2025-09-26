using Datalake.Inventory.Extensions;
using Datalake.Inventory.InMemory.Queries;
using Datalake.InventoryService.Database;
using Datalake.InventoryService.Database.Tables;
using Datalake.InventoryService.InMemory.Models;
using Datalake.InventoryService.InMemory.Stores;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Sources;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Datalake.InventoryService.InMemory.Repositories;

/// <summary>
/// Репозиторий источников данных
/// </summary>
public class SourcesMemoryRepository(DatalakeDataStore dataStore)
{
	#region API

	/// <summary>
	/// Создание нового источника
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceInfo">Параметры нового источника</param>
	/// <returns>Идентификатор нового источника</returns>
	public async Task<SourceInfo> CreateAsync(
		UserAccessEntity user,
		SourceInfo? sourceInfo = null)
	{
		user.ThrowIfNoGlobalAccess(AccessType.Manager);

		if (sourceInfo != null)
			return await ProtectedCreateAsync(user.Guid, sourceInfo);

		return await ProtectedCreateAsync(user.Guid);
	}

	/// <summary>
	/// Получение информации об источнике
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Информация об источнике</returns>
	public SourceInfo Get(UserAccessEntity user, int id)
	{
		var rule = user.GetAccessToSource(id);
		user.ThrowIfNoAccessToSource(AccessType.Viewer, id);

		var source = dataStore.State.SourcesInfo().FirstOrDefault(x => x.Id == id)
			?? throw new NotFoundException(message: "источник #" + id);

		source.AccessRule = new(rule.Id, rule.Access);

		return source;
	}

	/// <summary>
	/// Получение информации об источнике, включая теги, зависящие от него
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Информация об источнике</returns>
	public SourceWithTagsInfo GetWithTags(UserAccessEntity user, int id)
	{
		user.ThrowIfNoAccessToSource(AccessType.Viewer, id);

		var source = dataStore.State.SourcesInfoWithTags().FirstOrDefault(x => x.Id == id)
			?? throw new NotFoundException(message: "источник #" + id);

		var sourceRule = user.GetAccessToSource(id);
		source.AccessRule = new(sourceRule.Id, sourceRule.Access);

		foreach (var tag in source.Tags)
		{
			var tagRule = user.GetAccessToTag(tag.Id);
			tag.AccessRule = new(tagRule.Id, tagRule.Access);

			if (!tagRule.HasAccess(AccessType.Viewer))
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
	public SourceInfo[] GetAll(UserAccessEntity user, bool withCustom)
	{
		var sources = dataStore.State.SourcesInfo(withCustom).ToArray();

		List<SourceInfo> sourcesWithAccess = [];
		foreach (var source in sources)
		{
			var sourceRule = user.GetAccessToSource(source.Id);
			if (sourceRule.HasAccess(AccessType.Viewer))
			{
				source.AccessRule = new(sourceRule.Id, sourceRule.Access);
				sourcesWithAccess.Add(source);
			}
		}

		return sourcesWithAccess.ToArray();
	}

	/// <summary>
	/// Изменение параметров источника
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <param name="request">Новые параметры источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> UpdateAsync(
		UserAccessEntity user,
		int id,
		SourceUpdateRequest request)
	{
		user.ThrowIfNoAccessToSource(AccessType.Editor, id);

		return await ProtectedUpdateAsync(user.Guid, id, request);
	}

	/// <summary>
	/// Удаление источника
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(
		UserAccessEntity user,
		int id)
	{
		user.ThrowIfNoAccessToSource(AccessType.Manager, id);

		return await ProtectedDeleteAsync(user.Guid, id);
	}

	#endregion API

	#region Действия

	internal async Task<SourceInfo> ProtectedCreateAsync(Guid userGuid)
	{
		return await dataStore.ExecuteAtomicUpdateAsync<SourceInfo>((state, db) =>
		{
			SourceInfo result = new();

			DatalakeDataState stateUpdate(Source createdSource) => state with
			{
				Sources = state.Sources.Add(createdSource),
			};

			async Task<(Source createdEntity, SourceInfo result)> UpdateDatabase()
			{
				Source newSource = new(SourceType.Inopc);

				await db.Sources.AddAsync(newSource);
				await db.SaveChangesAsync();

				newSource.Name = $"Новый источник #{newSource.Id}";
				await db.SaveChangesAsync();

				await LogAsync(db, userGuid, newSource.Id, "Создан источник: " + newSource.Name);

				result.Id = newSource.Id;
				result.Name = newSource.Name;
				result.Address = newSource.Address;
				result.Type = newSource.Type;
				result.Description = newSource.Description;
				result.IsDisabled = newSource.IsDisabled;

				return (newSource, result);
			}

			return (result, stateUpdate, UpdateDatabase);
		});
	}

	internal async Task<SourceInfo> ProtectedCreateAsync(Guid userGuid, SourceInfo sourceInfo)
	{
		return await dataStore.ExecuteAtomicUpdateAsync<SourceInfo>((state, db) =>
		{
			if (state.Sources.Any(x => !x.IsDeleted && x.Name.Equals(sourceInfo.Name, StringComparison.OrdinalIgnoreCase)))
				throw new AlreadyExistException("Уже существует источник с таким именем");

			Source newSource = new(sourceInfo.Type, sourceInfo.Address, sourceInfo.Name, sourceInfo.Description);

			var result = new SourceInfo
			{
				Id = newSource.Id,
				Name = newSource.Name,
				Address = newSource.Address,
				Type = newSource.Type,
				Description = newSource.Description,
				IsDisabled = newSource.IsDisabled,
			};

			var newState = state with
			{
				Sources = state.Sources.Add(newSource),
			};

			async Task UpdateDatabase()
			{
				await db.Sources.AddAsync(newSource);
				await db.SaveChangesAsync(); // Получаем ID здесь

				// Обновляем результат с реальным ID
				result.Id = newSource.Id;

				await LogAsync(db, userGuid, newSource.Id, "Создан источник: " + sourceInfo.Name);
			}

			return (result, newState, UpdateDatabase);
		});
	}

	internal async Task<bool> ProtectedUpdateAsync(Guid userGuid, int id, SourceUpdateRequest request)
	{
		return await dataStore.ExecuteAtomicUpdateAsync<bool>((state, db) =>
		{
			if (!state.SourcesById.TryGetValue(id, out var currentSource))
				throw new NotFoundException($"Источник #{id} не найден");

			if (state.Sources.Any(x => !x.IsDeleted && x.Id != id && x.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
				throw new AlreadyExistException("Уже существует источник с таким именем");

			var updatedSource = currentSource with
			{
				Name = request.Name,
				Address = request.Address,
				Type = request.Type,
				Description = request.Description,
				IsDisabled = request.IsDisabled,
			};

			var newState = state with
			{
				Sources = state.Sources.Replace(currentSource, updatedSource),
			};

			// Действие для обновления БД
			async Task UpdateDatabase()
			{
				await db.Sources
					.Where(x => x.Id == id)
					.ExecuteUpdateAsync(x => x
						.SetProperty(p => p.Name, updatedSource.Name)
						.SetProperty(p => p.Description, updatedSource.Description)
						.SetProperty(p => p.Address, updatedSource.Address)
						.SetProperty(p => p.Type, updatedSource.Type)
						.SetProperty(p => p.IsDisabled, updatedSource.IsDisabled));

				await LogAsync(db, userGuid, id,
					$"Изменен источник: {currentSource.Name} -> {updatedSource.Name}",
					ObjectExtension.Difference(
						new { currentSource.Name, currentSource.Address, currentSource.Type, currentSource.IsDisabled },
						new { updatedSource.Name, updatedSource.Address, updatedSource.Type, updatedSource.IsDisabled }));
			}

			return (true, newState, UpdateDatabase);
		});
	}

	internal async Task<bool> ProtectedDeleteAsync(Guid userGuid, int id)
	{
		return await dataStore.ExecuteAtomicUpdateAsync<bool>((state, db) =>
		{
			if (!state.SourcesById.TryGetValue(id, out var currentSource))
				throw new NotFoundException($"Источник #{id} не найден");

			// Проверки, не требующие стейта
			Source updatedSource = currentSource with
			{
				IsDeleted = true,
			};

			var newState = state with
			{
				Sources = state.Sources.Replace(currentSource, updatedSource),
			};

			async Task UpdateDatabase()
			{
				await db.Sources
					.Where(x => x.Id == id)
					.ExecuteUpdateAsync(x => x.SetProperty(p => p.IsDeleted, updatedSource.IsDeleted));

				await db.SaveChangesAsync();

				await LogAsync(db, userGuid, id, "Удален источник: " + currentSource.Name + ".");
			}

			return (true, newState, UpdateDatabase);
		});
	}

	private static async Task LogAsync(InventoryEfContext db, Guid userGuid, int id, string message, string? details = null)
	{
		await db.Logs.AddAsync(new Log(
			LogCategory.Source,
			LogType.Success,
			userGuid,
			message,
			details,
			sourceId: id));

		await db.SaveChangesAsync();
	}

	#endregion Действия
}