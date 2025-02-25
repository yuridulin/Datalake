using Datalake.Database.Constants;
using Datalake.Database.Enums;
using Datalake.Database.Exceptions;
using Datalake.Database.Extensions;
using Datalake.Database.Models.AccessRights;
using Datalake.Database.Models.Auth;
using Datalake.Database.Models.Blocks;
using Datalake.Database.Models.UserGroups;
using Datalake.Database.Models.Users;
using Datalake.Database.Tables;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с блоками
/// </summary>
public class BlocksRepository(DatalakeContext db)
{
	#region Действия

	/// <summary>
	/// Создание нового блока
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="blockInfo">Параметры нового блока</param>
	/// <param name="parentId">Идентификатор родительского блока</param>
	/// <returns>Идентификатор нового блока</returns>
	public async Task<int> CreateAsync(
		UserAuthInfo user,
		BlockFullInfo? blockInfo = null,
		int? parentId = null)
	{
		if (parentId.HasValue)
		{
			AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Admin, parentId.Value);
		}
		else
		{
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);
		}

		User = user.Guid;

		return blockInfo != null ? await CreateAsync(blockInfo) : await CreateAsync(parentId);
	}

	/// <summary>
	/// Получение списка блоков с учетом уровня доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Список блоков с уровнями доступа к ним</returns>
	public async Task<BlockWithTagsInfo[]> ReadAllAsync(
		UserAuthInfo user)
	{
		BlockWithTagsInfo[] blocks = await GetBlocks(user);

		return blocks.Where(x => x.AccessRule.AccessType.HasAccess(AccessType.Viewer)).ToArray();
	}

	/// <summary>
	/// Получение полной информации о блоке, включая права доступа, поля и дочерние блоки
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <returns>Полная информация о блоке</returns>
	/// <exception cref="NotFoundException">Блок не найден</exception>
	public async Task<BlockFullInfo> ReadAsync(
		UserAuthInfo user,
		int id)
	{
		var rule = AccessRepository.GetAccessToBlock(user, id);
		if (!rule.AccessType.HasAccess(AccessType.Viewer))
			throw Errors.NoAccess;

		var block = await QueryFullInfo(id);

		block.AccessRule = rule;

		return block;
	}

	/// <summary>
	/// Получение дерева блоков с учетом уровня доступа
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <returns>Дерево блоков с уровнями доступа к ним</returns>
	public async Task<BlockTreeInfo[]> ReadAllAsTreeAsync(
		UserAuthInfo user)
	{
		BlockWithTagsInfo[] blocks = await GetBlocks(user);

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
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="block">Новые параметры блока</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> UpdateAsync(
		UserAuthInfo user,
		int id,
		BlockUpdateRequest block)
	{
		AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Admin, id);
		User = user.Guid;

		return await UpdateAsync(id, block);
	}

	/// <summary>
	/// Изменение расположения блока в иерархии
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <param name="parentId"></param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> MoveAsync(
		UserAuthInfo user,
		int id,
		int? parentId)
	{
		AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Admin, id);

		if (parentId.HasValue)
		{
			AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Admin, parentId.Value);
		}
		else
		{
			AccessRepository.ThrowIfNoGlobalAccess(user, AccessType.Admin);
		}
		User = user.Guid;

		return await MoveAsync(id, parentId);
	}

	/// <summary>
	/// Удаление блока
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор блока</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(
		UserAuthInfo user,
		int id)
	{
		AccessRepository.ThrowIfNoAccessToBlock(user, AccessType.Admin, id);
		User = user.Guid;

		return await DeleteAsync(id);
	}

	#endregion

	#region Реализация

	Guid? User { get; set; } = null;

	internal async Task<int> CreateAsync(int? parentId = null)
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

		await LogAsync(id.Value, "Создан блок: " + name);

		AccessRepository.Update();

		return id.Value;
	}

	internal async Task<int> CreateAsync(BlockFullInfo block)
	{
		if (block.Parent != null)
		{
			if (!await db.Blocks.AnyAsync(x => x.Id == block.Parent.Id))
				throw new NotFoundException($"Родительский блок #{block.Parent.Id} не найден");

			if (await db.Blocks.AnyAsync(x => x.ParentId == block.Parent.Id && x.Name == block.Name))
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

		await LogAsync(id.Value, "Создан блок: " + block.Name);

		AccessRepository.Update();

		return id ?? throw new DatabaseException(message: "не удалось создать блок", DatabaseStandartError.IdIsNull);
	}

	internal async Task<bool> UpdateAsync(int id, BlockUpdateRequest block)
	{
		var oldBlock = await QueryFullInfo(id);

		if (await db.Blocks.AnyAsync(x => x.Id != id && x.ParentId == oldBlock.ParentId && x.Name == block.Name))
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

		await LogAsync(id, "Изменен блок: " + block.Name, ObjectExtension.Difference(
			new { oldBlock.Name, oldBlock.Description, Tags = oldBlock.Tags.Select(t => t.Id) },
			new { block.Name, block.Description, Tags = block.Tags.Select(t => t.Id) }));

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	internal async Task<bool> MoveAsync(int id, int? parentId)
	{
		using var transaction = await db.BeginTransactionAsync();

		var block = await db.Blocks
			.Where(x => x.Id == id)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException(message: "блок " + id);

		await db.Blocks
			.Where(x => x.Id == id)
			.Set(x => x.ParentId, parentId == 0 ? null : parentId)
			.UpdateAsync();

		await LogAsync(id, "Изменено расположение блока: " + block.Name, ObjectExtension.Difference(
			new { block.ParentId },
			new { ParentId = parentId == 0 ? null : parentId }));

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	internal async Task<bool> DeleteAsync(int id)
	{
		using var transaction = await db.BeginTransactionAsync();

		var blockName = await db.Blocks
			.Where(x => x.Id == id)
			.Select(x => x.Name)
			.FirstOrDefaultAsync();

		await db.Blocks
			.Where(x => x.Id == id)
			.DeleteAsync();

		await LogAsync(id, "Удален блок: " + blockName);

		await transaction.CommitAsync();

		AccessRepository.Update();

		return true;
	}

	private async Task<BlockWithTagsInfo[]> GetBlocks(UserAuthInfo user)
	{
		var blocks = await QuerySimpleInfo().ToArrayAsync();
		foreach (var block in blocks)
		{
			block.AccessRule = AccessRepository.GetAccessToBlock(user, block.Id);
		}

		return blocks;
	}

	private async Task LogAsync(int id, string message, string? details = null)
	{
		await db.InsertAsync(new Log
		{
			Category = LogCategory.Blocks,
			RefId = id.ToString(),
			UserGuid = User,
			Text = message,
			Type = LogType.Success,
			Details = details,
		});
	}

	#endregion

	#region Запросы

	internal IQueryable<BlockWithTagsInfo> QuerySimpleInfo()
	{
		var query =
			from block in db.Blocks
			select new BlockWithTagsInfo
			{
				Id = block.Id,
				Guid = block.GlobalId,
				Name = block.Name,
				Description = block.Description,
				ParentId = block.ParentId,
				Tags = (
					from block_tag in db.BlockTags.InnerJoin(x => x.BlockId == block.Id)
					from tag in db.Tags.InnerJoin(x => x.Id == block_tag.TagId)
					from source in db.Sources.LeftJoin(x => x.Id == tag.SourceId)
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

	internal async Task<BlockFullInfo> QueryFullInfo(int id)
	{
		var parentsCte = db.GetCte<BlockTreeInfo>(cte =>
		{
			return (
				from x in db.Blocks
				where x.Id == id
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
