using Datalake.Database.Constants;
using Datalake.Database.Extensions;
using Datalake.Database.Tables;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.AccessRights;
using Datalake.PublicApi.Models.Auth;
using Datalake.PublicApi.Models.Blocks;
using Datalake.PublicApi.Models.UserGroups;
using Datalake.PublicApi.Models.Users;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с блоками
/// </summary>
public static class BlocksRepository
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
	public static async Task<int> CreateAsync(
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

		return blockInfo != null ? await VerifiedCreateAsync(db, user.Guid, blockInfo) : await VerifiedCreateAsync(db, user.Guid, parentId);
	}

	/// <summary>
	/// Получение списка блоков с учетом уровня доступа
	/// </summary>
	/// <param name="db">Текущий контекс базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Список блоков с уровнями доступа к ним</returns>
	public static async Task<BlockWithTagsInfo[]> ReadAllAsync(
		DatalakeContext db,
		UserAuthInfo user)
	{
		BlockWithTagsInfo[] blocks = await GetBlocks(db, user);

		return blocks.Where(x => x.AccessRule.AccessType.HasAccess(AccessType.Viewer)).ToArray();
	}

	/// <summary>
	/// Получение полной информации о блоке, включая права доступа, поля и дочерние блоки
	/// </summary>
	/// <param name="db">Текущий контекс базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <returns>Полная информация о блоке</returns>
	/// <exception cref="NotFoundException">Блок не найден</exception>
	public static async Task<BlockFullInfo> ReadAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int id)
	{
		var rule = AccessRepository.GetAccessToBlock(user, id);
		if (!rule.AccessType.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var block = await QueryFullInfo(db, id);

		block.AccessRule = rule;

		return block;
	}

	/// <summary>
	/// Получение дерева блоков с учетом уровня доступа
	/// </summary>
	/// <param name="db">Текущий контекс базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Дерево блоков с уровнями доступа к ним</returns>
	public static async Task<BlockTreeInfo[]> ReadAllAsTreeAsync(
		DatalakeContext db,
		UserAuthInfo user)
	{
		BlockWithTagsInfo[] blocks = await GetBlocks(db, user);

		return ReadChildren(null, string.Empty);

		BlockTreeInfo[] ReadChildren(int? id, string prefix)
		{
			var prefixString = prefix + (string.IsNullOrEmpty(prefix) ? string.Empty : ".");
			return blocks
				.Where(x => x.ParentId == id)
				.Select(x =>
				{
					var block = new BlockTreeInfo
					{
						Id = x.Id,
						Guid = x.Guid,
						ParentId = x.ParentId,
						Name = x.Name,
						FullName = prefixString + x.Name,
						Description = x.Description,
						Tags = x.Tags
							.Select(tag => new BlockNestedTagInfo
							{
								Guid = tag.Guid,
								Name = tag.Name,
								Id = tag.Id,
								Relation = tag.Relation,
								SourceId = tag.SourceId,
								LocalName = tag.LocalName,
								Type = tag.Type,
								Frequency = tag.Frequency,
								SourceType = tag.SourceType,
							})
							.ToArray(),
						AccessRule = x.AccessRule,
						Children = ReadChildren(x.Id, prefixString + x.Name),
					};

					if (!x.AccessRule.AccessType.HasAccess(AccessType.Viewer))
					{
						block.Name = string.Empty;
						block.Description = string.Empty;
						block.Tags = [];
					}

					return block;
				})
				.Where(x => x.Children.Length > 0 || x.AccessRule.AccessType.HasAccess(AccessType.Viewer))
				.ToArray();
		}
	}

	/// <summary>
	/// Изменение параметров блока, включая закрепленные теги
	/// </summary>
	/// <param name="db">Текущий контекс базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="block">Новые параметры блока</param>
	/// <returns>Флаг успешного завершения</returns>
	public static async Task<bool> UpdateAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int id,
		BlockUpdateRequest block)
	{
		AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Manager, id);

		return await VerifiedUpdateAsync(db, user.Guid, id, block);
	}

	/// <summary>
	/// Изменение расположения блока в иерархии
	/// </summary>
	/// <param name="db">Текущий контекс базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="parentId"></param>
	/// <returns>Флаг успешного завершения</returns>
	public static async Task<bool> MoveAsync(
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

		return await VerifiedMoveAsync(db, user.Guid, id, parentId);
	}

	/// <summary>
	/// Удаление блока
	/// </summary>
	/// <param name="db">Текущий контекс базы данных</param>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <returns>Флаг успешного завершения</returns>
	public static async Task<bool> DeleteAsync(
		DatalakeContext db,
		UserAuthInfo user,
		int id)
	{
		AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Admin, id);

		return await VerifiedDeleteAsync(db, user.Guid, id);
	}

	#endregion

	#region Реализация

	internal static async Task<int> VerifiedCreateAsync(DatalakeContext db, Guid userGuid, int? parentId = null)
	{
		int? id = await db.Blocks
			.Value(x => x.GlobalId, Guid.NewGuid())
			.Value(x => x.ParentId, parentId)
			.Value(x => x.Name, "INSERTING BLOCK")
			.Value(x => x.Description, string.Empty)
			.InsertWithInt32IdentityAsync();

		if (!id.HasValue)
			throw new DatabaseException(message: "не удалось создать блок", DatabaseStandartError.IdIsNull);

		string name = "Блок #" + id.Value;

		await db.Blocks
			.Where(x => x.Id == id.Value)
			.Set(x => x.Name, name)
			.UpdateAsync();

		await LogAsync(db, userGuid, id.Value, "Создан блок: " + name);

		AccessRepository.Update();

		return id.Value;
	}

	internal static async Task<int> VerifiedCreateAsync(DatalakeContext db, Guid userGuid, BlockFullInfo block)
	{
		if (block.Parent != null)
		{
			if (!await BlocksNotDeleted(db).AnyAsync(x => x.Id == block.Parent.Id))
				throw new NotFoundException($"Родительский блок #{block.Parent.Id} не найден");

			if (await BlocksNotDeleted(db).AnyAsync(x => x.ParentId == block.Parent.Id && x.Name == block.Name))
				throw new AlreadyExistException("Блок с таким именем уже существует");
		}

		int? id = await db.Blocks
			.Value(x => x.GlobalId, Guid.NewGuid())
			.Value(x => x.ParentId, block.Parent?.Id)
			.Value(x => x.Name, block.Name)
			.Value(x => x.Description, block.Description)
			.InsertWithInt32IdentityAsync();

		if (!id.HasValue)
			throw new DatabaseException(message: "не удалось создать блок", DatabaseStandartError.IdIsNull);

		await LogAsync(db, userGuid, id.Value, "Создан блок: " + block.Name);

		AccessRepository.Update();

		return id ?? throw new DatabaseException(message: "не удалось создать блок", DatabaseStandartError.IdIsNull);
	}

	internal static async Task<bool> VerifiedUpdateAsync(DatalakeContext db, Guid userGuid, int id, BlockUpdateRequest block)
	{
		var oldBlock = await QueryFullInfo(db, id);

		if (await BlocksNotDeleted(db).AnyAsync(x => x.Id != id && x.ParentId == oldBlock.ParentId && x.Name == block.Name))
			throw new AlreadyExistException("Блок с таким именем уже существует");

		using var transaction = await db.BeginTransactionAsync();

		int count = 0;

		count += await db.Blocks
			.Where(x => x.Id == id)
			.Set(x => x.Name, block.Name)
			.Set(x => x.Description, block.Description)
			.UpdateAsync();

		await db.BlockTags
			.Where(x => x.BlockId == id)
			.DeleteAsync();

		if (block.Tags.Length > 0)
		{
			await db.BlockTags.BulkCopyAsync(block.Tags.Select(x => new BlockTag
			{
				BlockId = id,
				TagId = x.Id,
				Name = x.Name,
				Relation = x.Relation,
			}));
		}

		await LogAsync(db, userGuid, id, "Изменен блок: " + block.Name, ObjectExtension.Difference(
			new { oldBlock.Name, oldBlock.Description, Tags = oldBlock.Tags.Select(t => t.Id) },
			new { block.Name, block.Description, Tags = block.Tags.Select(t => t.Id) }));

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	internal static async Task<bool> VerifiedMoveAsync(DatalakeContext db, Guid userGuid, int id, int? parentId)
	{
		using var transaction = await db.BeginTransactionAsync();

		var block = await BlocksNotDeleted(db)
			.Where(x => x.Id == id)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "блок " + id);

		await db.Blocks
			.Where(x => x.Id == id)
			.Set(x => x.ParentId, parentId == 0 ? null : parentId)
			.UpdateAsync();

		await LogAsync(db, userGuid, id, "Изменено расположение блока: " + block.Name, ObjectExtension.Difference(
			new { block.ParentId },
			new { ParentId = parentId == 0 ? null : parentId }));

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	internal static async Task<bool> VerifiedDeleteAsync(DatalakeContext db, Guid userGuid, int id)
	{
		using var transaction = await db.BeginTransactionAsync();

		var blockName = await BlocksNotDeleted(db)
			.Where(x => x.Id == id)
			.Select(x => x.Name)
			.FirstOrDefaultAsync();

		await db.Blocks
			.Where(x => x.Id == id)
			.Set(x => x.IsDeleted, true)
			.UpdateAsync();

		await LogAsync(db, userGuid, id, "Удален блок: " + blockName);

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	private static async Task<BlockWithTagsInfo[]> GetBlocks(DatalakeContext db, UserAuthInfo user)
	{
		var blocks = await QuerySimpleInfo(db).ToArrayAsync();
		foreach (var block in blocks)
		{
			block.AccessRule = AccessRepository.GetAccessToBlock(user, block.Id);
		}

		return blocks;
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

	#endregion

	#region Запросы

	internal static IQueryable<Block> BlocksNotDeleted(DatalakeContext db)
	{
		return db.Blocks.Where(x => !x.IsDeleted);
	}

	internal static IQueryable<BlockWithTagsInfo> QuerySimpleInfo(DatalakeContext db)
	{
		var query =
			from block in db.Blocks
			where !block.IsDeleted
			select new BlockWithTagsInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				Name = block.Name,
				Description = block.Description,
				ParentId = block.ParentId,
				Tags = (
					from block_tag in db.BlockTags.InnerJoin(x => x.BlockId == block.Id)
					from tag in db.Tags.InnerJoin(x => x.Id == block_tag.TagId && !x.IsDeleted)
					from source in db.Sources.LeftJoin(x => x.Id == tag.SourceId && !x.IsDeleted)
					select new BlockNestedTagInfo
					{
						Id = tag.Id,
						Name = tag.Name,
						Guid = tag.GlobalGuid,
						Relation = block_tag.Relation,
						LocalName = block_tag.Name ?? tag.Name,
						Type = tag.Type,
						Frequency = tag.Frequency,
						SourceId = tag.SourceId,
						SourceType = source == null ? SourceType.NotSet : source.Type,
					}
				).ToArray(),
			};

		return query;
	}

	internal static async Task<BlockFullInfo> QueryFullInfo(DatalakeContext db, int id)
	{
		var parentsCte = db.GetCte<BlockTreeInfo>(cte =>
		{
			return (
				from x in db.Blocks
				where !x.IsDeleted && x.Id == id
				select new BlockTreeInfo
				{
					Id = x.Id,
					Guid = x.GlobalId,
					Name = x.Name,
					ParentId = x.ParentId,
				}
			).Concat(
				from x in db.Blocks
				join p in cte on x.Id equals p.ParentId
				where !x.IsDeleted
				select new BlockTreeInfo
				{
					Id = x.Id,
					Guid = x.GlobalId,
					Name = x.Name,
					ParentId = x.ParentId,
				}
			);
		});

		var query =
			from block in db.Blocks.Where(x => x.Id == id)
			from parent in db.Blocks.LeftJoin(x => x.Id == block.ParentId)
			where !block.IsDeleted && !parent.IsDeleted
			select new BlockFullInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				Name = block.Name,
				Description = block.Description,
				Parent = parent == null ? null : new BlockFullInfo.BlockParentInfo
				{
					Id = parent.Id,
					Name = parent.Name
				},
				Adults = parentsCte.Where(x => x.Id != id).ToArray(),
				Children = (
					from child in db.Blocks.LeftJoin(x => x.ParentId == block.Id)
					where !child.IsDeleted
					select new BlockFullInfo.BlockChildInfo
					{
						Id = child.Id,
						Name = child.Name
					}
				).ToArray(),
				Properties = (
					from property in db.BlockProperties.LeftJoin(x => x.BlockId == block.Id)
					select new BlockFullInfo.BlockPropertyInfo
					{
						Id = property.Id,
						Name = property.Name,
						Type = property.Type,
						Value = property.Value,
					}
				).ToArray(),
				Tags = (
					from block_tag in db.BlockTags.InnerJoin(x => x.BlockId == block.Id)
					from tag in db.Tags.LeftJoin(x => x.Id == block_tag.TagId)
					from source in db.Sources.LeftJoin(x => x.Id == tag.SourceId)
					where !tag.IsDeleted && !source.IsDeleted
					select new BlockNestedTagInfo
					{
						Id = tag.Id,
						Name = block_tag.Name ?? "",
						Guid = tag.GlobalGuid,
						Relation = block_tag.Relation,
						LocalName = tag.Name,
						Type = tag.Type,
						Frequency = tag.Frequency,
						SourceType = source == null ? SourceType.NotSet : source.Type,
					}
				).ToArray(),
				AccessRights = (
					from rights in db.AccessRights.InnerJoin(x => x.BlockId == block.Id)
					from user in db.Users.LeftJoin(x => x.Guid == rights.UserGuid)
					from usergroup in db.UserGroups.LeftJoin(x => x.Guid == rights.UserGroupGuid)
					where !user.IsDeleted && !usergroup.IsDeleted
					select new AccessRightsForObjectInfo
					{
						Id = rights.Id,
						IsGlobal = rights.IsGlobal,
						AccessType = rights.AccessType,
						User = user == null ? null : new UserSimpleInfo
						{
							Guid = user.Guid,
							FullName = user.FullName ?? string.Empty,
						},
						UserGroup = usergroup == null ? null : new UserGroupSimpleInfo
						{
							Guid = usergroup.Guid,
							Name = usergroup.Name,
						},
					}
				).ToArray(),
			};

		return await query.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "блок #" + id);
	}

	#endregion
}
