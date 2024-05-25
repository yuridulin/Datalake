using DatalakeDatabase.ApiModels.Sources;
using DatalakeDatabase.Enums;
using DatalakeDatabase.Exceptions;
using DatalakeDatabase.Extensions;
using LinqToDB;

namespace DatalakeDatabase.Repositories;

public partial class SourcesRepository(DatalakeContext db)
{
	public async Task<int> CreateAsync()
	{
		var transaction = await db.BeginTransactionAsync();

		int? id = await db.Sources
			.Value(x => x.Name, "INSERTING")
			.Value(x => x.Address, "")
			.Value(x => x.Type, SourceType.Unknown)
			.InsertWithInt32IdentityAsync();

		if (!id.HasValue)
			throw new DatabaseException("Не удалось добавить источник");

		await db.Sources
			.Where(x => x.Id == id.Value)
			.Set(x => x.Name, ValueChecker.RemoveWhitespaces("Новый источник #" + id.Value, "_"))
			.UpdateAsync();

		await db.SetLastUpdateToNowAsync();

		await transaction.CommitAsync();

		return id.Value;
	}

	public async Task<int> CreateAsync(SourceInfo sourceInfo)
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
			throw new DatabaseException("Не удалось добавить источник");

		await db.SetLastUpdateToNowAsync();

		await transaction.CommitAsync();

		return id.Value;
	}

	public async Task<bool> UpdateAsync(int id, SourceInfo sourceInfo)
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
			throw new DatabaseException($"Не удалось обновить источник #{id}");

		await db.SetLastUpdateToNowAsync();

		await transaction.CommitAsync();

		return true;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		using var transaction = await db.BeginTransactionAsync();

		var count = await db.Sources
			.Where(x => x.Id == id)
			.DeleteAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось удалить источник #{id}");

		// при удалении источника его теги становятся ручными
		await db.Tags
			.Where(x => x.SourceId == id)
			.Set(x => x.SourceId, (int)CustomSource.Manual)
			.UpdateAsync();

		await db.SetLastUpdateToNowAsync();

		await transaction.CommitAsync();

		return true;
	}
}
