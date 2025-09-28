using Datalake.InventoryService.Application.Features.Audit.Queries.Audit;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database;
using Datalake.PrivateApi.Attributes;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Models.Sources;

namespace Datalake.InventoryService.Application.Features.Sources;

[Scoped]
public class SourcesService(
	InventoryEfContext db,
	SourcesRepository sourcesRepository,
	GetAuditQueryHandler logsRepository,
	InventoryCacheStore dataStore)
{
	/// <summary>
	/// Создание нового источника
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceInfo">Параметры нового источника</param>
	/// <returns>Идентификатор нового источника</returns>
	public async Task<int> CreateAsync(
		UserAccessEntity user,
		SourceInfo? sourceInfo = null)
	{
		user.ThrowIfNoGlobalAccess(AccessType.Manager);

		SourceEntity createdSource;

		using var transaction = await db.Database.BeginTransactionAsync();
		try
		{
			var result = sourceInfo != null
				? await sourcesRepository.CreateAsync(sourceInfo)
				: await sourcesRepository.CreateEmptyAsync();

			createdSource = result.AddedEntities.First();

			await transaction.CommitAsync();
		}
		catch (Exception)
		{
			await transaction.RollbackAsync();
			throw;
		}

		using (await dataStore.AcquireWriteLockAsync())
		{
			dataStore.UpdateStateWithinLock(state => state with
			{
				Sources = state.Sources
					.Add(createdSource)
			});
		}

		return createdSource.Id;
	}

	/// <summary>
	/// Изменение параметров источника
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <param name="request">Новые параметры источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> UpdateAsync(
		UserAccessEntity user,
		int sourceId,
		SourceUpdateRequest request)
	{
		user.ThrowIfNoAccessToSource(AccessType.Editor, sourceId);

		SourceEntity updatedSource;

		using var transaction = await db.Database.BeginTransactionAsync();
		try
		{
			var result = await sourcesRepository.UpdateAsync(sourceId, request);

			updatedSource = result.UpdatedEntities.First();

			await logsRepository.AddSourceLogAsync(user.Guid, sourceId, "Изменен источник: " + result.UpdatedEntities.First().Name + ".");

			await transaction.CommitAsync();
		}
		catch (Exception)
		{
			await transaction.RollbackAsync();
			throw;
		}

		using (await dataStore.AcquireWriteLockAsync())
		{
			dataStore.UpdateStateWithinLock(state => state with
			{
				Sources = state.Sources // TODO: сюда просится словарь
					.RemoveAll(x => x.Id == updatedSource.Id)
					.Add(updatedSource)
			});
		}

		return true;
	}

	/// <summary>
	/// Удаление источника
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceId">Идентификатор источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(
		UserAccessEntity user,
		int sourceId)
	{
		user.ThrowIfNoAccessToSource(AccessType.Manager, sourceId);

		SourceEntity deletedSource;

		using var transaction = await db.Database.BeginTransactionAsync();
		try
		{
			var result = await sourcesRepository.DeleteAsync(sourceId);

			deletedSource = result.UpdatedEntities.First();

			await logsRepository.AddSourceLogAsync(user.Guid, sourceId, "Удален источник: " + deletedSource.Name + ".");

			await transaction.CommitAsync();
		}
		catch (Exception)
		{
			await transaction.RollbackAsync();
			throw;
		}

		using (await dataStore.AcquireWriteLockAsync())
		{
			dataStore.UpdateStateWithinLock(state => state with
			{
				Sources = state.Sources // TODO: сюда просится словарь
					.RemoveAll(x => x.Id == deletedSource.Id)
					.Add(deletedSource)
			});
		}

		return true;
	}
}
