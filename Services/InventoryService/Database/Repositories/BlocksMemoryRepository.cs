using Datalake.Inventory.Constants;
using Datalake.Inventory.Extensions;
using Datalake.Inventory.InMemory.Queries;
using Datalake.InventoryService.Database;
using Datalake.InventoryService.Database.Tables;
using Datalake.InventoryService.InMemory.Models;
using Datalake.InventoryService.InMemory.Stores;
using Datalake.PrivateApi.Entities;
using Datalake.PrivateApi.ValueObjects;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Blocks;
using Microsoft.EntityFrameworkCore;
using System.Collections.Immutable;

namespace Datalake.Inventory.InMemory.Repositories;

/// <summary>
/// Репозиторий работы с блоками в памяти приложения
/// </summary>
public class BlocksMemoryRepository(DatalakeDataStore dataStore)
{
	#region API

	/// <summary>
	/// Создание нового блока
	/// </summary>
	/// <param name="db">Текущий контекс базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="blockInfo">Параметры нового блока</param>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <returns>Идентификатор нового блока</returns>
	public async Task<BlockWithTagsInfo> CreateAsync(
		InventoryEfContext db,
		UserAccessEntity user,
		BlockFullInfo? blockInfo = null,
		int? parentId = null)
	{
		if (parentId.HasValue)
		{
			user.ThrowIfNoAccessToBlock(AccessType.Manager, parentId.Value);
		}
		else
		{
			user.ThrowIfNoGlobalAccess(AccessType.Manager);
		}

		return blockInfo != null ? await ProtectedCreateAsync(db, user.Guid, blockInfo) : await ProtectedCreateAsync(db, user.Guid, parentId);
	}

	/// <summary>
	/// Получение списка блоков с учетом уровня доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Список блоков с уровнями доступа к ним</returns>
	public BlockWithTagsInfo[] GetAll(UserAccessEntity user)
	{
		var blocks = dataStore.State.BlocksInfoWithTags();

		List<BlockWithTagsInfo> blocksWithAccess = [];
		foreach (var block in blocks)
		{
			var blockRule = user.GetAccessToBlock(block.Id);
			if (blockRule.HasAccess(AccessType.Viewer))
			{
				block.AccessRule = new(blockRule.Id, blockRule.Access);
				blocksWithAccess.Add(block);
			}
		}

		return blocksWithAccess.ToArray();
	}

	/// <summary>
	/// Получение полной информации о блоке, включая права доступа, поля и дочерние блоки
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <returns>Полная информация о блоке</returns>
	/// <exception cref="NotFoundException">Блок не найден</exception>
	public BlockFullInfo Get(UserAccessEntity user, int id)
	{
		var blockRule = user.GetAccessToBlock(id);
		if (!blockRule.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var block = dataStore.State.BlockInfoWithParentsAndTags(id);

		block.AccessRule = new(blockRule.Id, blockRule.Access);

		return block;
	}

	/// <summary>
	/// Получение дерева блоков с учетом уровня доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Дерево блоков с уровнями доступа к ним</returns>
	public BlockTreeInfo[] GetAllAsTree(UserAccessEntity user)
	{
		var blocks = GetAll(user);

		return GetChildren(null, string.Empty);

		BlockTreeInfo[] GetChildren(int? parentId, string prefix) => blocks
			.Where(x => x.ParentId == parentId)
			.Select(x => new
			{
				Node = x,
				Children = GetChildren(x.Id, AppendPrefix(prefix, x.Name))
			})
			.Select(p =>
			{
				var rule = new AccessRule(p.Node.AccessRule.RuleId, p.Node.AccessRule.Access);
				var hasViewer = rule.HasAccess(AccessType.Viewer);

				if (!hasViewer)
					return null!;

				if (p.Children.Length == 0)
					return null!;

				return new BlockTreeInfo
				{
					Id = p.Node.Id,
					Guid = p.Node.Guid,
					ParentId = p.Node.ParentId,
					Name = hasViewer ? p.Node.Name : string.Empty,
					FullName = AppendPrefix(prefix, p.Node.Name),
					Description = hasViewer ? p.Node.Description : string.Empty,
					Tags = hasViewer ? p.Node.Tags : Array.Empty<BlockNestedTagInfo>(),
					AccessRule = p.Node.AccessRule,
					Children = p.Children
				};
			})
			.Where(x => x != null)
			.OrderBy(x => x.Name)
			.ToArray();
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
		InventoryEfContext db,
		UserAccessEntity user,
		int id,
		BlockUpdateRequest block)
	{
		user.ThrowIfNoAccessToBlock(AccessType.Manager, id);

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
		InventoryEfContext db,
		UserAccessEntity user,
		int id,
		int? parentId)
	{
		user.ThrowIfNoAccessToBlock(AccessType.Manager, id);

		if (parentId.HasValue)
		{
			user.ThrowIfNoAccessToBlock(AccessType.Manager, parentId.Value);
		}
		else
		{
			user.ThrowIfNoGlobalAccess(AccessType.Manager);
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
		InventoryEfContext db,
		UserAccessEntity user,
		int id)
	{
		user.ThrowIfNoAccessToBlock(AccessType.Manager, id);

		return await ProtectedDeleteAsync(db, user.Guid, id);
	}

	#endregion API

	#region Действия

	internal async Task<BlockWithTagsInfo> ProtectedCreateAsync(InventoryEfContext db, Guid userGuid, int? parentId = null)
	{
		// Проверки, не требующие стейта
		Block createdBlock = new(
			Guid.NewGuid(),
			parentId);

		// Блокируем стейт до завершения обновления
		DatalakeDataState currentState;
		using (await dataStore.AcquireWriteLockAsync())
		{
			currentState = dataStore.State;

			// Проверки на актуальном стейте

			// Обновление в БД
			using var transaction = await db.Database.BeginTransactionAsync();

			try
			{
				await db.Blocks.AddAsync(createdBlock);
				await db.SaveChangesAsync();

				createdBlock.Name = "Блок #" + createdBlock.Id;

				await LogAsync(db, userGuid, createdBlock.Id, "Создан блок: " + createdBlock.Name);

				await db.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось создать блок в БД", ex);
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

	internal async Task<BlockWithTagsInfo> ProtectedCreateAsync(InventoryEfContext db, Guid userGuid, BlockFullInfo block)
	{
		// Проверки, не требующие стейта
		Block createdBlock = new(Guid.NewGuid(), block.ParentId, block.Name, block.Description);

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
			using var transaction = await db.Database.BeginTransactionAsync();

			try
			{
				await db.Blocks.AddAsync(createdBlock);
				await db.SaveChangesAsync();

				await LogAsync(db, userGuid, createdBlock.Id, "Создан блок: " + block.Name);
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось создать блок в БД", ex);
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

	internal async Task<bool> ProtectedUpdateAsync(InventoryEfContext db, Guid userGuid, int id, BlockUpdateRequest request)
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
				.Select(x => new BlockTag(id, x.Id, x.Name, x.Relation))
				.ToArray();

			// Обновление в БД

			using var transaction = await db.Database.BeginTransactionAsync();
			try
			{
				int count = 0;

				count += await db.Blocks
					.Where(x => x.Id == id)
					.ExecuteUpdateAsync(x => x
						.SetProperty(p => p.Name, request.Name)
						.SetProperty(p => p.Description, request.Description));

				await db.BlockTags
					.Where(x => x.BlockId == id)
					.ExecuteDeleteAsync();

				if (newTagsRelations.Length > 0)
					await db.BlockTags.AddRangeAsync(newTagsRelations);

				await LogAsync(db, userGuid, id, "Изменен блок: " + request.Name, ObjectExtension.Difference(
					new { oldBlock.Name, oldBlock.Description, },
					new { updatedBlock.Name, updatedBlock.Description, }));

				await db.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось обновить блок в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Blocks = state.Blocks.Replace(oldBlock, updatedBlock),
				BlockTags = state.BlockTags.Where(x => x.BlockId != id).Concat(newTagsRelations).ToImmutableList(),
			});
		}

		// Возвращение ответа
		return true;
	}

	internal async Task<bool> ProtectedMoveAsync(InventoryEfContext db, Guid userGuid, int id, int? parentId)
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

			if (parentId.HasValue && parentId.Value != 0 && !currentState.BlocksById.ContainsKey(parentId.Value))
				throw new NotFoundException($"Родительский блок #{parentId.Value} не найден");

			if (parentId.HasValue && parentId.Value == id)
				throw new InvalidValueException("Блок не может быть родителем самому себе");

			updatedBlock = oldBlock with { ParentId = parentId == 0 ? null : parentId };

			// Обновление в БД
			using var transaction = await db.Database.BeginTransactionAsync();

			try
			{
				await db.Blocks
					.Where(x => x.Id == id)
					.ExecuteUpdateAsync(x => x.SetProperty(p => p.ParentId, updatedBlock.ParentId));

				await LogAsync(db, userGuid, id, "Изменено расположение блока: " + oldBlock.Name, ObjectExtension.Difference(
					new { oldBlock.ParentId },
					new { updatedBlock.ParentId }));

				await db.SaveChangesAsync();
				await transaction.CommitAsync();
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();
				throw new Exception("Не удалось переместить блок в БД", ex);
			}

			// Обновление стейта в случае успешного обновления БД
			dataStore.UpdateStateWithinLock(state => state with
			{
				Blocks = state.Blocks.Replace(oldBlock, updatedBlock),
			});
		}

		// Возвращение ответа
		return true;
	}

	internal async Task<bool> ProtectedDeleteAsync(InventoryEfContext db, Guid userGuid, int id)
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
			using var transaction = await db.Database.BeginTransactionAsync();

			try
			{
				await db.Blocks
					.Where(x => x.Id == id)
					.ExecuteUpdateAsync(x => x.SetProperty(p => p.IsDeleted, updatedBlock.IsDeleted));

				await LogAsync(db, userGuid, id, "Удален блок: " + oldBlock.Name);

				await db.SaveChangesAsync();
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
				Blocks = state.Blocks.Replace(oldBlock, updatedBlock),
			});
		}

		// Возвращение ответа
		return true;
	}

	private static async Task LogAsync(InventoryEfContext db, Guid userGuid, int id, string message, string? details = null)
	{
		await db.Logs.AddAsync(new Log(
			LogCategory.Blocks,
			LogType.Success,
			userGuid,
			message,
			details,
			blockId: id));
	}

	private static string AppendPrefix(string prefix, string name) =>
		string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";

	#endregion Действия
}