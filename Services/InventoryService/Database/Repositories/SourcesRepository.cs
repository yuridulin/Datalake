using Datalake.InventoryService.Database.Models;
using Datalake.InventoryService.Database.Tables;
using Datalake.InventoryService.InMemory.Stores;
using Datalake.PrivateApi.Attributes;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Sources;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Database.Repositories;

[Scoped]
public class SourcesService(
	InventoryEfContext db,
	SourcesRepository sourcesRepository,
	LogsRepository logsRepository,
	DatalakeDataStore dataStore)
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

		Source createdSource;

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

		Source updatedSource;

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

		Source deletedSource;

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

/// <summary>
/// Репозиторий источников данных
/// </summary>
[Scoped]
public class SourcesRepository(InventoryEfContext db)
{
	public async Task<DatabaseResult<Source>> CreateEmptyAsync()
	{
		Source newSource = new(SourceType.Inopc);

		await db.Sources.AddAsync(newSource);
		await db.SaveChangesAsync();

		newSource.Name = $"Новый источник #{newSource.Id}";
		await db.SaveChangesAsync();

		return new()
		{
			AddedEntities = [newSource]
		};
	}

	public async Task<DatabaseResult<Source>> CreateAsync(SourceInfo sourceInfo)
	{
		if (await db.Sources.AsNoTracking().AnyAsync(x => !x.IsDeleted && x.Name == sourceInfo.Name))
			throw new InvalidOperationException("Уже существует источник с таким именем");

		Source newSource = new(sourceInfo.Type, sourceInfo.Address, sourceInfo.Name, sourceInfo.Description);

		await db.Sources.AddAsync(newSource);
		await db.SaveChangesAsync();

		return new()
		{
			AddedEntities = [newSource]
		};
	}

	public async Task<DatabaseResult<Source>> UpdateAsync(int sourceId, SourceUpdateRequest request)
	{
		var existSource = await db.Sources.FirstOrDefaultAsync(x => x.Id == sourceId && !x.IsDeleted)
			?? throw new NotFoundException($"Источник #{sourceId} не найден");

		if (await db.Sources.AsNoTracking().AnyAsync(x => !x.IsDeleted && x.Id != sourceId && x.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
			throw new AlreadyExistException("Уже существует источник с таким именем");

		existSource.Name = request.Name;
		existSource.Address = request.Address;
		existSource.Type = request.Type;
		existSource.Description = request.Description;
		existSource.IsDisabled = request.IsDisabled;

		await db.SaveChangesAsync();

		return new()
		{
			UpdatedEntities = [existSource]
		};
	}

	public async Task<DatabaseResult<Source>> DeleteAsync(int sourceId)
	{
		var existSource = await db.Sources.FirstOrDefaultAsync(x => x.Id == sourceId)
			?? throw new NotFoundException($"Источник #{sourceId} не найден");

		existSource.IsDeleted = true;

		await db.SaveChangesAsync();

		return new()
		{
			UpdatedEntities = [existSource]
		};
	}
}
