using Datalake.Inventory.InMemory.Queries;
using Datalake.InventoryService.Database;
using Datalake.InventoryService.Database.Tables;
using Datalake.InventoryService.InMemory.Models;
using Datalake.InventoryService.InMemory.Stores;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.AccessRights;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Datalake.InventoryService.InMemory.Repositories;

/// <summary>
/// Репозиторий работы с правами доступа в памяти приложения
/// </summary>
public class AccessRightsMemoryRepository(DatalakeDataStore dataStore)
{
	#region API

	/// <summary>
	/// Применение изменений прав доступа
	/// </summary>
	/// <param name="db">Текущий контекст базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="request">Новые права доступа</param>
	public async Task ApplyChangesAsync(InventoryEfContext db, UserAccessEntity user, AccessRightsApplyRequest request)
	{
		user.ThrowIfNoGlobalAccess(AccessType.Admin);

		await ProtectedApplyChangesAsync(db, request);
	}

	/// <summary>
	/// Получение списка правил доступа для запрошенных объектов
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="userGuid">Идентификатор пользователя</param>
	/// <param name="userGroupGuid">Идентификатор группы пользователей</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="blockId">Идентификатор блока</param>
	/// <param name="tagId">Идентификатор тега</param>
	/// <returns>Список правил доступа</returns>
	public AccessRightsInfo[] Get(
		UserAccessEntity user,
		Guid? userGuid,
		Guid? userGroupGuid,
		int? sourceId,
		int? blockId,
		int? tagId)
	{
		user.ThrowIfNoGlobalAccess(AccessType.Editor);

		return dataStore.State.AccessRightsInfo(userGuid, userGroupGuid, sourceId, blockId, tagId)
			.ToArray();
	}

	#endregion

	#region Действия

	internal async Task ProtectedApplyChangesAsync(InventoryEfContext db, AccessRightsApplyRequest request)
	{
		// Проверки, не требующие стейта
		AccessRights[] newRules = [];
		if (request.UserGroupGuid.HasValue)
		{
			newRules = request.Rights
				.Select(x => new AccessRights(null, request.UserGroupGuid, false, x.TagId, x.SourceId, x.BlockId, x.AccessType))
				.ToArray();
		}
		else if (request.UserGuid.HasValue)
		{
			newRules = request.Rights
				.Select(x => new AccessRights(request.UserGuid, null, false, x.TagId, x.SourceId, x.BlockId, x.AccessType))
				.ToArray();
		}
		else if (request.SourceId.HasValue)
		{
			newRules = request.Rights
				.Select(x => new AccessRights(request.UserGuid, request.UserGroupGuid, false, null, request.SourceId, null, x.AccessType))
				.ToArray();
		}
		else if (request.BlockId.HasValue)
		{
			newRules = request.Rights
				.Select(x => new AccessRights(request.UserGuid, request.UserGroupGuid, false, null, null, request.BlockId, x.AccessType))
				.ToArray();
		}
		else if (request.TagId.HasValue)
		{
			newRules = request.Rights
				.Select(x => new AccessRights(request.UserGuid, request.UserGroupGuid, false, request.TagId, null, null, x.AccessType))
				.ToArray();
		}

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			AccessRights[] oldRules = [];
			if (request.UserGroupGuid.HasValue)
				oldRules = currentState.AccessRights.Where(x => !x.IsGlobal && x.UserGroupGuid == request.UserGroupGuid).ToArray();

			else if (request.UserGuid.HasValue)
				oldRules = currentState.AccessRights.Where(x => !x.IsGlobal && x.UserGuid == request.UserGuid).ToArray();

			else if (request.SourceId.HasValue)
				oldRules = currentState.AccessRights.Where(x => !x.IsGlobal && x.SourceId == request.SourceId).ToArray();

			else if (request.BlockId.HasValue)
				oldRules = currentState.AccessRights.Where(x => !x.IsGlobal && x.BlockId == request.BlockId).ToArray();

			else if (request.TagId.HasValue)
				oldRules = currentState.AccessRights.Where(x => !x.IsGlobal && x.TagId == request.TagId).ToArray();

			// Обновление в БД
			using var transaction = await db.Database.BeginTransactionAsync();

			try
			{
				if (oldRules.Length > 0)
					await db.AccessRights.Where(x => oldRules.Select(r => r.Id).Contains(x.Id)).ExecuteDeleteAsync();

				if (newRules.Length > 0)
					await db.AccessRights.AddRangeAsync(newRules);

				if (request.UserGroupGuid.HasValue)
					newRules = await db.AccessRights.Where(x => !x.IsGlobal && x.UserGroupGuid == request.UserGroupGuid).ToArrayAsync();

				else if (request.UserGuid.HasValue)
					newRules = await db.AccessRights.Where(x => !x.IsGlobal && x.UserGuid == request.UserGuid).ToArrayAsync();

				else if (request.SourceId.HasValue)
					newRules = await db.AccessRights.Where(x => !x.IsGlobal && x.SourceId == request.SourceId).ToArrayAsync();

				else if (request.BlockId.HasValue)
					newRules = await db.AccessRights.Where(x => !x.IsGlobal && x.BlockId == request.BlockId).ToArrayAsync();

				else if (request.TagId.HasValue)
					newRules = await db.AccessRights.Where(x => !x.IsGlobal && x.TagId == request.TagId).ToArrayAsync();

				await db.SaveChangesAsync();

				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось обновить права в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				AccessRights = state.AccessRights.RemoveRange(oldRules).AddRange(newRules),
			});
		}

		// Возвращение ответа
	}

	#endregion
}
