using Datalake.Database.Enums;
using Datalake.Database.Exceptions;
using Datalake.Database.Extensions;
using Datalake.Database.Models.Auth;
using Datalake.Database.Models.Sources;
using Datalake.Database.Tables;
using LinqToDB;

namespace Datalake.Database.Repositories;

/// <summary>
/// Репозиторий для работы с источниками данных
/// </summary>
public partial class SourcesRepository(DatalakeContext db)
{
	#region Действия

	/// <summary>
	/// Создание нового источника
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="sourceInfo">Параметры нового источника</param>
	/// <returns>Идентификатор нового источника</returns>
	public async Task<int> CreateAsync(
		UserAuthInfo user,
		SourceInfo? sourceInfo = null)
	{
		AccessRepository.CheckGlobalAccess(user.Rights, AccessType.Admin);
		User = user.Guid;

		if (sourceInfo != null)
			return await CreateAsync(sourceInfo);

		return await CreateAsync();
	}

	/// <summary>
	/// Изменение параметров источника
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <param name="sourceInfo">Новые параметры источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> UpdateAsync(
		UserAuthInfo user,
		int id,
		SourceInfo sourceInfo)
	{
		AccessRepository.CheckAccessToSource(user.Rights, AccessType.Admin, id);
		User = user.Guid;

		return await UpdateAsync(id, sourceInfo);
	}

	/// <summary>
	/// Удаление источника
	/// </summary>
	/// <param name="user">Информация о пользователе</param>
	/// <param name="id">Идентификатор источника</param>
	/// <returns>Флаг успешного завершения</returns>
	public async Task<bool> DeleteAsync(
		UserAuthInfo user,
		int id)
	{
		AccessRepository.CheckAccessToSource(user.Rights, AccessType.Admin, id);
		User = user.Guid;

		return await DeleteAsync(id);
	}

	#endregion

	#region Реализация

	Guid User { get; set; }

	internal async Task<int> CreateAsync()
	{
		var transaction = await db.BeginTransactionAsync();

		int? id = await db.Sources
			.Value(x => x.Name, "INSERTING")
			.Value(x => x.Address, "")
			.Value(x => x.Type, SourceType.Unknown)
			.InsertWithInt32IdentityAsync();

		string name = ValueChecker.RemoveWhitespaces("Новый источник #" + id.Value, "_");

		await db.Sources
			.Where(x => x.Id == id.Value)
			.Set(x => x.Name, name)
			.UpdateAsync();

		await LogAsync(id.Value, "Создан источник: " + name);

		await transaction.CommitAsync();

		SystemRepository.Update();
		AccessRepository.Update();

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

		await LogAsync(id.Value, "Создан источник: " + sourceInfo.Name);

		await transaction.CommitAsync();

		SystemRepository.Update();
		AccessRepository.Update();

		return id.Value;
	}

	internal async Task<bool> UpdateAsync(int id, SourceInfo sourceInfo)
	{
		sourceInfo.Name = ValueChecker.RemoveWhitespaces(sourceInfo.Name, "_");

		var source = await db.Sources
			.Where(x => x.Id == id)
			.FirstOrDefaultAsync()
			?? throw new NotFoundException($"Источник #{id} не найден");

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

		await LogAsync(id, "Изменен источник: " + sourceInfo.Name, ObjectExtension.Difference(
			new { source.Name, source.Address, source.Type },
			new { sourceInfo.Name, sourceInfo.Address, sourceInfo.Type }));

		await transaction.CommitAsync();

		SystemRepository.Update();

		return true;
	}

	internal async Task<bool> DeleteAsync(int id)
	{
		using var transaction = await db.BeginTransactionAsync();

		var name = await db.Sources
			.Where(x => x.Id == id)
			.Select(x => x.Name)
			.FirstOrDefaultAsync();

		var count = await db.Sources
			.Where(x => x.Id == id)
			.DeleteAsync();

		if (count == 0)
			throw new DatabaseException($"Не удалось удалить источник #{id}", DatabaseStandartError.DeletedZero);

		// при удалении источника его теги становятся ручными
		int tagsCount = await db.Tags
			.Where(x => x.SourceId == id)
			.Set(x => x.SourceId, (int)CustomSource.Manual)
			.UpdateAsync();

		await LogAsync(id, "Удален источник: " + name + ". Затронуто тегов: " + tagsCount);

		await transaction.CommitAsync();

		SystemRepository.Update();
		AccessRepository.Update();

		return true;
	}

	internal async Task LogAsync(int id, string message, string? details = null)
	{
		await db.InsertAsync(new Log
		{
			Category = LogCategory.Source,
			RefId = id.ToString(),
			UserGuid = User,
			Text = message,
			Type = LogType.Success,
			Details = details,
		});
	}

	#endregion
}
