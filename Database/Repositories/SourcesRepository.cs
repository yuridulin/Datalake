using Datalake.ApiClasses.Enums;
using Datalake.ApiClasses.Exceptions;
using Datalake.ApiClasses.Models.Sources;
using Datalake.ApiClasses.Models.Users;
using Datalake.Database.Extensions;
using LinqToDB;

namespace Datalake.Database.Repositories;

public partial class SourcesRepository(DatalakeContext db)
{
	#region Действия

	public async Task<int> CreateAsync(
		UserAuthInfo user,
		SourceInfo? sourceInfo = null)
	{
		await db.AccessRepository.CheckGlobalAccess(user, AccessType.Admin);

		if (sourceInfo != null)
			return await CreateAsync(sourceInfo);

		return await CreateAsync();
	}

	public async Task<bool> UpdateAsync(
		UserAuthInfo user,
		int id,
		SourceInfo sourceInfo)
	{
		await db.AccessRepository.CheckAccessToSource(user, AccessType.Admin, id);

		return await UpdateAsync(id, sourceInfo);
	}

	public async Task<bool> DeleteAsync(
		UserAuthInfo user,
		int id)
	{
		await db.AccessRepository.CheckAccessToSource(user, AccessType.Admin, id);

		return await DeleteAsync(id);
	}

	#endregion

	#region Реализация

	internal async Task<int> CreateAsync()
	{
		var transaction = await db.BeginTransactionAsync();

		int? id = await db.Sources
			.Value(x => x.Name, "INSERTING")
			.Value(x => x.Address, "")
			.Value(x => x.Type, SourceType.Unknown)
			.InsertWithInt32IdentityAsync();

		if (!id.HasValue)
			throw new DatabaseException(message: "не удалось добавить источник", DatabaseStandartError.IdIsNull);

		await db.Sources
			.Where(x => x.Id == id.Value)
			.Set(x => x.Name, ValueChecker.RemoveWhitespaces("Новый источник #" + id.Value, "_"))
			.UpdateAsync();

		db.SetLastUpdateToNow();
		await transaction.CommitAsync();

		return id.Value;
	}

	internal async Task<int> CreateAsync(SourceInfo sourceInfo)
	{
		sourceInfo.Name = ValueChecker.RemoveWhitespaces(sourceInfo.Name, "_");

		if (await db.Sources.AnyAsync(x => x.Name == sourceInfo.Name))
			throw new AlreadyExistException("Уже существует источник с таким именем");

		if (sourceInfo.Type == SourceType.Custom)
			throw new InvalidValueException("Нельзя добавить системный источник");

		var transaction = await db.BeginTransactionAsync();

		int? id = await db.Sources
			.Value(x => x.Name, sourceInfo.Name)
			.Value(x => x.Address, sourceInfo.Address)
			.Value(x => x.Type, sourceInfo.Type)
			.InsertWithInt32IdentityAsync();

		if (!id.HasValue)
			throw new DatabaseException("Не удалось добавить источник", DatabaseStandartError.IdIsNull);

		db.SetLastUpdateToNow();
		await transaction.CommitAsync();

		return id.Value;
	}

	internal async Task<bool> UpdateAsync(int id, SourceInfo sourceInfo)
	{
		sourceInfo.Name = ValueChecker.RemoveWhitespaces(sourceInfo.Name, "_");

		if (!await db.Sources.AnyAsync(x => x.Id == id))
			throw new NotFoundException($"Источник #{id} не найден");

		if (await db.Sources.AnyAsync(x => x.Name == sourceInfo.Name && x.Id != id))
			throw new AlreadyExistException("Уже существует источник с таким именем");

		var transaction = await db.BeginTransactionAsync();

		int count = await db.Sources
			.Where(x => x.Id == id)
			.Set(x => x.Name, sourceInfo.Name)
			.Set(x => x.Address, sourceInfo.Address)
			.Set(x => x.Type, sourceInfo.Type)
			.UpdateAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось обновить источник #{id}", DatabaseStandartError.UpdatedZero);

		db.SetLastUpdateToNow();
		await transaction.CommitAsync();

		return true;
	}

	internal async Task<bool> DeleteAsync(int id)
	{
		using var transaction = await db.BeginTransactionAsync();

		var count = await db.Sources
			.Where(x => x.Id == id)
			.DeleteAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось удалить источник #{id}", DatabaseStandartError.DeletedZero);

		// при удалении источника его теги становятся ручными
		await db.Tags
			.Where(x => x.SourceId == id)
			.Set(x => x.SourceId, (int)CustomSource.Manual)
			.UpdateAsync();

		db.SetLastUpdateToNow();
		await transaction.CommitAsync();

		return true;
	}

	#endregion
}
