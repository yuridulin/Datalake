using Datalake.Database.Extensions;
using Datalake.Database.Repositories;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Blocks;
using LinqToDB;
using LinqToDB.Data;
using System.Collections.Immutable;

namespace Datalake.Database.InMemory.Repositories;

/// <summary>
/// Репозиторий работы с блоками в памяти приложения
/// </summary>
public class BlocksMemoryRepository(DatalakeDataStore dataStore)
{
	#region Действия

	/// <summary>
	/// Создание нового блока
	/// </summary>
	/// <param name="db">Текущий контекс базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="blockInfo">Параметры нового блока</param>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <returns>Идентификатор нового блока</returns>
	public async Task<BlockWithTagsInfo> CreateAsync(
		DatalakeContext db,
		UserAuthInfo user,
		BlockFullInfo? blockInfo = null,
		int? parentId = null)
	{
		if (parentId.HasValue)
		{
			AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Manager, parentId.Value);
		}
		else
		{
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Manager);
		}

		return blockInfo != null ? await ProtectedCreateAsync(db, user.Guid, blockInfo) : await ProtectedCreateAsync(db, user.Guid, parentId);
	}

	/// <summary>
	/// Изменение параметров блока, включая закрепленные теги
	/// </summary>
	/// <param name="db">Текущий контекс базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="block">Новые параметры блока</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> UpdateAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int id,
		BlockUpdateRequest block)
	{
		AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Manager, id);

		return await ProtectedUpdateAsync(db, user.Guid, id, block);
	}

	/// <summary>
	/// Изменение расположения блока в иерархии
	/// </summary>
	/// <param name="db">Текущий контекс базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="parentId"></param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> MoveAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int id,
		int? parentId)
	{
		AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Manager, id);

		if (parentId.HasValue)
		{
			AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Manager, parentId.Value);
		}
		else
		{
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Manager);
		}

		return await ProtectedMoveAsync(db, user.Guid, id, parentId);
	}

	/// <summary>
	/// Удаление блока
	/// </summary>
	/// <param name="db">Текущий контекс базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int id)
	{
		AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Manager, id);

		return await ProtectedDeleteAsync(db, user.Guid, id);
	}

	#endregion

	internal async Task<BlockWithTagsInfo> ProtectedCreateAsync(DatalakeContext db, Guid userGuid, int? parentId = null)
	{
		// Проверки, не требующие стейта
		Block createdBlock = new()
		{
			GlobalId = Guid.NewGuid(),
			ParentId = parentId,
			Name = "INSERTING BLOCK",
			Description = string.Empty,
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
				int id = await db.Blocks
					.Value(x => x.GlobalId, createdBlock.GlobalId)
					.Value(x => x.ParentId, createdBlock.ParentId)
					.Value(x => x.Name, createdBlock.Name)
					.Value(x => x.Description, createdBlock.Description)
					.InsertWithInt32IdentityAsync()
					?? throw new DatabaseException(message: "не удалось создать блок", DatabaseStandartError.IdIsNull);

				createdBlock.Id = id;
				createdBlock.Name = "Блок #" + id;

				await db.Blocks
					.Where(x => x.Id == createdBlock.Id)
					.Set(x => x.Name, createdBlock.Name)
					.UpdateAsync();

				await LogAsync(db, userGuid, createdBlock.Id, "Создан блок: " + createdBlock.Name);

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
				Blocks = state.Blocks.Add(createdBlock),
			});
		}

		// Возвращение ответа
		var info = new BlockWithTagsInfo
		{
			Guid = createdBlock.GlobalId,
			Id = createdBlock.Id,
			Name = createdBlock.Name,
			Description = createdBlock.Description,
			ParentId = createdBlock.ParentId,
		};

		return info;
	}

	internal async Task<BlockWithTagsInfo> ProtectedCreateAsync(DatalakeContext db, Guid userGuid, BlockFullInfo block)
	{
		// Проверки, не требующие стейта
		Block createdBlock = new()
		{
			GlobalId = Guid.NewGuid(),
			Name = block.Name,
			Description = block.Description,
			ParentId = block.ParentId,
			IsDeleted = false,
		};

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (createdBlock.ParentId.HasValue && !currentState.BlocksById.ContainsKey(createdBlock.ParentId.Value))
				throw new NotFoundException($"Родительский блок #{createdBlock.ParentId.Value} не найден");

			if (currentState.Blocks.Any(x => !x.IsDeleted && x.ParentId == createdBlock.ParentId && x.Name == createdBlock.Name))
				throw new AlreadyExistException("Блок с таким именем уже существует");

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				int id = await db.Blocks
					.Value(x => x.GlobalId, createdBlock.GlobalId)
					.Value(x => x.ParentId, createdBlock.ParentId)
					.Value(x => x.Name, createdBlock.Name)
					.Value(x => x.Description, createdBlock.Description)
					.InsertWithInt32IdentityAsync()
					?? throw new DatabaseException(message: "не удалось создать блок", DatabaseStandartError.IdIsNull);

				createdBlock.Id = id;

				await LogAsync(db, userGuid, id, "Создан блок: " + block.Name);
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
				Blocks = state.Blocks.Add(createdBlock),
			});
		}

		// Возвращение ответа
		var info = new BlockWithTagsInfo
		{
			Guid = createdBlock.GlobalId,
			Id = createdBlock.Id,
			Name = createdBlock.Name,
			Description = createdBlock.Description,
			ParentId = createdBlock.ParentId,
		};

		return info;
	}

	internal async Task<bool> ProtectedUpdateAsync(DatalakeContext db, Guid userGuid, int id, BlockUpdateRequest request)
	{
		// Проверки, не требующие стейта
		Block updatedBlock;
		BlockTag[] newTagsRelations;

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.BlocksById.TryGetValue(id, out var oldBlock))
				throw new NotFoundException($"Блок #{id} не найден");

			if (currentState.Blocks.Any(x => !x.IsDeleted && x.Id != id && x.ParentId == oldBlock.ParentId && x.Name == request.Name))
				throw new AlreadyExistException("Блок с таким именем уже существует");

			updatedBlock = oldBlock with
			{
				Name = request.Name,
				Description = request.Description,
			};

			newTagsRelations = request.Tags
				.Select(x => new BlockTag
				{
					BlockId = id,
					TagId = x.Id,
					Name = x.Name,
					Relation = x.Relation,
				})
				.ToArray();

			// Обновление в БД

			using var transaction = await db.BeginTransactionAsync();
			try
			{
				int count = 0;

				count += await db.Blocks
					.Where(x => x.Id == id)
					.Set(x => x.Name, request.Name)
					.Set(x => x.Description, request.Description)
					.UpdateAsync();

				await db.BlockTags
					.Where(x => x.BlockId == id)
					.DeleteAsync();

				if (newTagsRelations.Length > 0)
					await db.BlockTags.BulkCopyAsync(newTagsRelations);

				await LogAsync(db, userGuid, id, "Изменен блок: " + request.Name, ObjectExtension.Difference(
					new { oldBlock.Name, oldBlock.Description, },
					new { updatedBlock.Name, updatedBlock.Description, }));

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
				Blocks = state.Blocks.Remove(oldBlock).Add(updatedBlock),
				BlockTags = state.BlockTags.Where(x => x.BlockId != id).Concat(newTagsRelations).ToImmutableList(),
			});
		}

		// Возвращение ответа
		return true;
	}

	internal async Task<bool> ProtectedMoveAsync(DatalakeContext db, Guid userGuid, int id, int? parentId)
	{
		// Проверки, не требующие стейта
		Block updatedBlock;

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.BlocksById.TryGetValue(id, out var oldBlock))
				throw new NotFoundException(message: "блок " + id);

			updatedBlock = oldBlock with { ParentId = parentId == 0 ? null : parentId };

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				await db.Blocks
					.Where(x => x.Id == id)
					.Set(x => x.ParentId, updatedBlock.ParentId)
					.UpdateAsync();

				await LogAsync(db, userGuid, id, "Изменено расположение блока: " + oldBlock.Name, ObjectExtension.Difference(
					new { oldBlock.ParentId },
					new { updatedBlock.ParentId }));

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
				Blocks = state.Blocks.Remove(oldBlock).Add(updatedBlock),
			});
		}

		// Возвращение ответа
		return true;
	}

	internal async Task<bool> ProtectedDeleteAsync(DatalakeContext db, Guid userGuid, int id)
	{
		// Проверки, не требующие стейта
		Block updatedBlock;

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте
			if (!currentState.BlocksById.TryGetValue(id, out var oldBlock))
				throw new NotFoundException(message: "блок " + id);

			updatedBlock = oldBlock with { IsDeleted = true };

			// Обновление в БД
			using var transaction = await db.BeginTransactionAsync();

			try
			{
				await db.Blocks
					.Where(x => x.Id == id)
					.Set(x => x.IsDeleted, updatedBlock.IsDeleted)
					.UpdateAsync();

				await LogAsync(db, userGuid, id, "Удален блок: " + oldBlock.Name);

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
				Blocks = state.Blocks.Remove(oldBlock).Add(updatedBlock),
			});
		}

		// Возвращение ответа
		return true;
	}

	private static async Task LogAsync(DatalakeContext db, Guid userGuid, int id, string message, string? details = null)
	{
		await db.InsertAsync(new Log
		{
			Category = LogCategory.Blocks,
			RefId = id.ToString(),
			AffectedBlockId = id,
			AuthorGuid = userGuid,
			Text = message,
			Type = LogType.Success,
			Details = details,
		});
	}

}
