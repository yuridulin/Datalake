using Datalake.Database.Enums;
using Datalake.Database.Exceptions;
using Datalake.Database.Extensions;
using Datalake.Database.Models.Auth;
using Datalake.Database.Models.Blocks;
using Datalake.Database.Tables;
using LinqToDB;
using LinqToDB.Data;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с блоками
/// </summary>
public partial class BlocksRepository(DatalakeContext db)
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
			AccessRepository.CheckAccessToBlock(user.Rights, AccessType.Admin, parentId.Value);
		}
		else
		{
			AccessRepository.CheckGlobalAccess(user.Rights, AccessType.Admin);
		}
		
		User = user.Guid;

		return blockInfo != null ? await CreateAsync(blockInfo) : await CreateAsync(parentId);
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
		AccessRepository.CheckAccessToBlock(user.Rights, AccessType.Admin, id);
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
		AccessRepository.CheckAccessToBlock(user.Rights, AccessType.Admin, id);
		if (parentId.HasValue)
		{
			AccessRepository.CheckAccessToBlock(user.Rights, AccessType.Admin, parentId.Value);
		}
		else
		{
			AccessRepository.CheckGlobalAccess(user.Rights, AccessType.Admin);
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
		AccessRepository.CheckAccessToBlock(user.Rights, AccessType.Admin, id);
		User = user.Guid;

		return await DeleteAsync(id);
	}

	#endregion

	#region Реализация

	Guid User { get; set; }

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
		if (await db.Blocks.AnyAsync(x => x.Name == block.Name))
			throw new AlreadyExistException("Блок с таким именем уже существует");

		if (block.Parent != null)
		{
			if (!await db.Blocks.AnyAsync(x => x.Id == block.Parent.Id))
				throw new NotFoundException($"Родительский блок #{block.Parent.Id} не найден");
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
		var oldBlock = await GetInfoWithAllRelations()
			.Where(x => x.Id == id)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Блок #{id} не найден");

		if (await db.Blocks.AnyAsync(x => x.Id != id && x.Name == block.Name))
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

	internal async Task LogAsync(int id, string message, string? details = null)
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
}
