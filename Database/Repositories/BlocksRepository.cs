using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Blocks;
using Datalake.ApiClasses.Models.Users;
using Datalake.Database.Models;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

public partial class BlocksRepository(DatalakeContext db)
{
	#region Действия

	public async Task<int> CreateAsync(
		UserAuthInfo user,
		BlockFullInfo? blockInfo = null,
		int? parentId = null)
	{
		await db.AccessRepository.CheckGlobalAccess(user, AccessType.Admin);

		return blockInfo != null ? await CreateAsync(blockInfo) : await CreateAsync(parentId);
	}

	public async Task<bool> UpdateAsync(
		UserAuthInfo user,
		int id,
		BlockUpdateRequest block)
	{
		await db.AccessRepository.CheckAccessToBlockAsync(user, AccessType.Admin, id);
		return await UpdateAsync(id, block);
	}

	public async Task<bool> MoveAsync(
		UserAuthInfo user,
		int id,
		int? parentId)
	{
		await db.AccessRepository.CheckAccessToBlockAsync(user, AccessType.Admin, id);
		return await MoveAsync(id, parentId);
	}

	public async Task<bool> DeleteAsync(
		UserAuthInfo user,
		int id)
	{
		await db.AccessRepository.CheckAccessToBlockAsync(user, AccessType.Admin, id);
		return await DeleteAsync(id);
	}

	#endregion

	#region Реализация

	internal async Task<int> CreateAsync(int? parentId = null)
	{
		int? id;

		try
		{
			id = await db.Blocks
				.Value(x => x.GlobalId, Guid.NewGuid())
				.Value(x => x.ParentId, parentId)
				.Value(x => x.Name, "INSERTING BLOCK")
				.Value(x => x.Description, string.Empty)
				.InsertWithInt32IdentityAsync();

			if (id.HasValue)
				await db.Blocks
					.Where(x => x.Id == id.Value)
					.Set(x => x.Name, "Блок #" + id.Value)
					.UpdateAsync();

			return id.Value;
		}
		catch (Exception ex)
		{
			throw new DatabaseException(message: "не удалось добавить блок", ex);
		}
	}

	internal async Task<int> CreateAsync(BlockFullInfo block)
	{
		if (await db.Blocks.AnyAsync(x => x.Name == block.Name))
			throw new AlreadyExistException("Блок с таким именем уже существует");
		if (block.Parent != null)
		{
			if (!await db.Blocks.AnyAsync(x => x.Id == block.Parent.Id))
				throw new NotFoundException($"Родительский блок #{block.Parent.Id} не найдена");
		}

		int? id;

		try
		{
			id = await db.Blocks
				.Value(x => x.GlobalId, Guid.NewGuid())
				.Value(x => x.ParentId, block.Parent?.Id)
				.Value(x => x.Name, block.Name)
				.Value(x => x.Description, block.Description)
				.InsertWithInt32IdentityAsync();
		}
		catch (Exception ex)
		{
			throw new DatabaseException(message: "не удалось добавить блок", ex);
		}

		return id ?? throw new DatabaseException(message: "не удалось добавить блок", DatabaseStandartError.IdIsNull);
	}

	internal async Task<bool> UpdateAsync(int id, BlockUpdateRequest block)
	{
		if (!await db.Blocks.AnyAsync(x => x.Id == id))
			throw new NotFoundException($"Сущность #{id} не найдена");
		if (await db.Blocks.AnyAsync(x => x.Id != id && x.Name == block.Name))
			throw new AlreadyExistException("Сущность с таким именем уже существует");

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

		await transaction.CommitAsync();

		if (count == 0)
			throw new DatabaseException(message: "не удалось обновить блок #{id}", DatabaseStandartError.UpdatedZero);

		return true;
	}

	internal async Task<bool> MoveAsync(int id, int? parentId)
	{
		using var transaction = await db.BeginTransactionAsync();

		try
		{
			await db.Blocks
				.Where(x => x.Id == id)
				.Set(x => x.ParentId, parentId == 0 ? null : parentId)
				.UpdateAsync();

			await transaction.CommitAsync();

			return true;
		}
		catch (Exception ex)
		{
			transaction.Rollback();
			throw new DatabaseException("не удалось переместить блок", ex);
		}
	}

	internal async Task<bool> DeleteAsync(int id)
	{
		var count = await db.Blocks
			.Where(x => x.Id == id)
			.DeleteAsync();

		if (count == 0)
			throw new DatabaseException(message: "не удалось удалить блок #{id}", DatabaseStandartError.DeletedZero);

		return true;
	}

	#endregion
}
