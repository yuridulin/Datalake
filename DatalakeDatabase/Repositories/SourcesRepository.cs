using DatalakeDatabase.ApiModels.Sources;
using DatalakeDatabase.Exceptions;
using LinqToDB;

namespace DatalakeDatabase.Repositories;

public partial class SourcesRepository(DatalakeContext db)
{
	public async Task<int> CreateAsync(SourceInfo sourceInfo)
	{
		if (await db.Sources.AnyAsync(x => x.Name == sourceInfo.Name))
			throw new AlreadyExistException("Уже существует источник с таким именем");

		if (sourceInfo.Type == Enums.SourceType.Custom)
			throw new InvalidValueException("Нельзя добавить источник, который не является общедоступным");

		int? id = await db.Sources
			.Value(x => x.Name, sourceInfo.Name)
			.Value(x => x.Address, sourceInfo.Address)
			.Value(x => x.Type, sourceInfo.Type)
			.InsertWithInt32IdentityAsync();

		if (!id.HasValue)
			throw new DatabaseException("Не удалось добавить источник");

		return id.Value;
	}

	public async Task<bool> UpdateAsync(int id, SourceInfo sourceInfo)
	{
		if (!await db.Sources.AnyAsync(x => x.Id == id))
			throw new NotFoundException($"Источник #{id} не найден");
		if (await db.Sources.AnyAsync(x => x.Name == sourceInfo.Name))
			throw new AlreadyExistException("Уже существует источник с таким именем");

		int count = await db.Sources
			.Where(x => x.Id == id)
			.Set(x => x.Name, sourceInfo.Name)
			.Set(x => x.Address, sourceInfo.Address)
			.Set(x => x.Type, sourceInfo.Type)
			.UpdateAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось обновить источник #{id}");

		return true;
	}

	public async Task<bool> DeleteAsync(int id)
	{
		var count = await db.Sources
			.Where(x => x.Id == id)
			.DeleteAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось удалить источник #{id}");

		return true;
	}
}
