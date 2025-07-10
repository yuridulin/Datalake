using Datalake.Database.Constants;
using Datalake.Database.Extensions;
using Datalake.Database.Functions;
using Datalake.Database.InMemory.Models;
using Datalake.Database.InMemory.Queries;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Blocks;
using LinqToDB;
using LinqToDB.Data;
using System.Collections.Immutable;
using System.Text;

namespace Datalake.Database.InMemory.Repositories;

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
		DatalakeContext db,
		UserAuthInfo user,
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
	public BlockWithTagsInfo[] ReadAll(UserAuthInfo user)
	{
		var blocks = dataStore.State.BlocksInfoWithTags();

		List<BlockWithTagsInfo> blocksWithAccess = [];
		foreach (var block in blocks)
		{
			block.AccessRule = user.GetAccessToBlock(block.Id);
			if (block.AccessRule.HasAccess(AccessType.Viewer))
				blocksWithAccess.Add(block);
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
	public BlockFullInfo Read(UserAuthInfo user, int id)
	{
		var rule = user.GetAccessToBlock(id);
		if (!rule.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var block = dataStore.State.BlockInfoWithParentsAndTags(id);

		block.AccessRule = rule;

		return block;
	}

	/// <summary>
	/// Получение дерева блоков с учетом уровня доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Дерево блоков с уровнями доступа к ним</returns>
	public BlockTreeInfo[] ReadAllAsTree(UserAuthInfo user)
	{
		var blocks = ReadAll(user);

		return ReadChildren(null, string.Empty);

		BlockTreeInfo[] ReadChildren(int? parentId, string prefix) => blocks
			.Where(x => x.ParentId == parentId)
			.Select(x => new {
				Node = x,
				Children = ReadChildren(x.Id, AppendPrefix(prefix, x.Name))
			})
			.Where(p => p.Node.AccessRule.HasAccess(AccessType.Viewer) || p.Children.Length > 0)
			.Select(p => {
				var hasViewer = p.Node.AccessRule.HasAccess(AccessType.Viewer);
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
		DatalakeContext db,
		UserAuthInfo user,
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
		DatalakeContext db,
		UserAuthInfo user,
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
		DatalakeContext db,
		UserAuthInfo user,
		int id)
	{
		user.ThrowIfNoAccessToBlock(AccessType.Manager, id);

		return await ProtectedDeleteAsync(db, user.Guid, id);
	}

	#endregion

	#region Действия

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
					await BulkCopyWithOutputAsync(db, newTagsRelations);

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

	private static string AppendPrefix(string prefix, string name) =>
		string.IsNullOrEmpty(prefix) ? name : $"{prefix}.{name}";

	private static async Task BulkCopyWithOutputAsync(DatalakeContext db, BlockTag[] newTagsRelations)
	{
		// Формируем параметризованный SQL запрос
		var insertSql = $"""
			INSERT INTO "{BlockTag.TableName}" 
			("BlockId", "TagId", "Name", "Relation")
			VALUES {string.Join(", ", newTagsRelations.Select((_, i) =>
				$"(:b{i}, :t{i}, :n{i}, :r{i})"))}
			RETURNING "Id";
		""";

		// Создаем параметры
		var parameters = new List<DataParameter>();
		for (var i = 0; i < newTagsRelations.Length; i++)
		{
			var item = newTagsRelations[i];
			parameters.Add(new DataParameter($"b{i}", item.BlockId));
			parameters.Add(new DataParameter($"t{i}", item.TagId));
			parameters.Add(new DataParameter($"n{i}", item.Name ?? ""));
			parameters.Add(new DataParameter($"r{i}", (int)item.Relation));
		}

		// Выполняем запрос и получаем Id
		var insertedIds = (await db.QueryToArrayAsync<int>(insertSql, parameters.ToArray()))
			.ToList();

		// Обновляем исходные объекты
		for (var i = 0; i < newTagsRelations.Length; i++)
		{
			newTagsRelations[i].Id = insertedIds[i];
		}
	}

	#endregion
}
