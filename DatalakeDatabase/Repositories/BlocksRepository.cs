using DatalakeApiClasses.Enums;
using DatalakeApiClasses.Exceptions;
using DatalakeApiClasses.Models.Blocks;
using DatalakeApiClasses.Models.Users;
using DatalakeDatabase.Extensions;
using DatalakeDatabase.Models;
using LinqToDB;
using LinqToDB.Data;

namespace DatalakeDatabase.Repositories;

public partial class BlocksRepository(DatalakeContext db)
{
	#region Действия

	public async Task<int> CreateAsync(UserAuthInfo user, BlockInfo? blockInfo = null)
	{
		await db.CheckAccessAsync(user, AccessType.Admin, AccessScope.Global);
		return blockInfo != null ? await CreateAsync(blockInfo) : await CreateAsync();
	}

	public async Task<bool> UpdateAsync(UserAuthInfo user, int id, BlockInfo block)
	{
		await db.CheckAccessAsync(user, AccessType.Admin, AccessScope.Block, id);
		return await UpdateAsync(id, block);
	}

	public async Task<bool> DeleteAsync(UserAuthInfo user, int id)
	{
		await db.CheckAccessAsync(user, AccessType.Admin, AccessScope.Block, id);
		return await DeleteAsync(id);
	}

	#endregion

	#region Реализация

	internal async Task<int> CreateAsync()
	{
		int? id;

		try
		{
			id = await db.Blocks
				.Value(x => x.GlobalId, Guid.NewGuid())
				.Value(x => x.ParentId, 0)
				.Value(x => x.Name, "INSERTING BLOCK")
				.Value(x => x.Description, string.Empty)
				.InsertWithInt32IdentityAsync();

			if (id.HasValue)
				await db.Blocks
					.Where(x => x.Id == id.Value)
					.Set(x => x.Name, "Сущность #" + id.Value)
					.UpdateAsync();

			return id.Value;
		}
		catch
		{
			throw new DatabaseException("Не удалось добавить сущность");
		}

	}

	internal async Task<int> CreateAsync(BlockInfo block)
	{
		if (await db.Blocks.AnyAsync(x => x.Name == block.Name))
			throw new AlreadyExistException("Сущность с таким именем уже существует");
		if (block.Parent != null)
		{
			if (!await db.Blocks.AnyAsync(x => x.Id == block.Parent.Id))
				throw new NotFoundException($"Родительская сущность #{block.Parent.Id} не найдена");
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
		catch
		{
			throw new DatabaseException("Не удалось добавить сущность");
		}

		return id ?? throw new DatabaseException("Не удалось добавить сущность");
	}

	internal async Task<bool> UpdateAsync(int id, BlockInfo block)
	{
		if (!await db.Blocks.AnyAsync(x => x.Id == id))
			throw new NotFoundException($"Сущность #{id} не найдена");
		if (await db.Blocks.AnyAsync(x => x.Name == block.Name))
			throw new AlreadyExistException("Сущность с таким именем уже существует");
		if (block.Parent != null)
		{
			if (!await db.Blocks.AnyAsync(x => x.Id == block.Parent.Id))
				throw new NotFoundException($"Родительская сущность #{block.Parent.Id} не найдена");
		}

		await db.BeginTransactionAsync();

		int count = 0;

		count += await db.Blocks
			.Where(x => x.Id == id)
			.Set(x => x.Name, block.Name)
			.Set(x => x.Description, block.Description)
			.Set(x => x.ParentId, block.Parent?.Id ?? null)
			.UpdateAsync();

		await UpdateRelationsWithTags(id, block.Tags);

		await db.CommitTransactionAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось обновить сущность #{id}");

		return true;
	}

	internal async Task<bool> DeleteAsync(int id)
	{
		var count = await db.Blocks
			.Where(x => x.Id == id)
			.DeleteAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось удалить сущность #{id}");

		return true;
	}

	private async Task<bool> UpdateRelationsWithTags(int id, BlockInfo.BlockTagInfo[]? tags)
	{
		await db.BlockTags
			.Where(x => x.BlockId == id)
			.DeleteAsync();

		if (tags != null && tags.Length > 0)
		{
			await db.BulkCopyAsync(tags.Select(x => new BlockTag
			{
				BlockId = id,
				TagId = x.Id,
				Name = x.Name,
			}));
		}

		return true;
	}

	#endregion
}
