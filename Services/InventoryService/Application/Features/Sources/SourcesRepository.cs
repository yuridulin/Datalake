using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database;
using Datalake.PrivateApi.Attributes;
using Datalake.PublicApi.Enums;
using Datalake.PublicApi.Exceptions;
using Datalake.PublicApi.Models.Sources;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Application.Features.Sources;

/// <summary>
/// Репозиторий источников данных
/// </summary>
[Scoped]
public class SourcesRepository(InventoryEfContext db)
{
	public async Task<DatabaseResult<SourceEntity>> CreateEmptyAsync()
	{
		SourceEntity newSource = new(SourceType.Inopc);

		await db.Sources.AddAsync(newSource);
		await db.SaveChangesAsync();

		newSource.Name = $"Новый источник #{newSource.Id}";
		await db.SaveChangesAsync();

		return new()
		{
			AddedEntities = [newSource]
		};
	}

	public async Task<DatabaseResult<SourceEntity>> CreateAsync(SourceInfo sourceInfo)
	{
		if (await db.Sources.AsNoTracking().AnyAsync(x => !x.IsDeleted && x.Name == sourceInfo.Name))
			throw new InvalidOperationException("Уже существует источник с таким именем");

		SourceEntity newSource = new(sourceInfo.Type, sourceInfo.Address, sourceInfo.Name, sourceInfo.Description);

		await db.Sources.AddAsync(newSource);
		await db.SaveChangesAsync();

		return new()
		{
			AddedEntities = [newSource]
		};
	}

	public async Task<DatabaseResult<SourceEntity>> UpdateAsync(int sourceId, SourceUpdateRequest request)
	{
		var existSource = await db.Sources.FirstOrDefaultAsync(x => x.Id == sourceId && !x.IsDeleted)
			?? throw new NotFoundException($"Источник #{sourceId} не найден");

		if (await db.Sources.AsNoTracking().AnyAsync(x => !x.IsDeleted && x.Id != sourceId && x.Name.Equals(request.Name, StringComparison.OrdinalIgnoreCase)))
			throw new AlreadyExistException("Уже существует источник с таким именем");

		existSource.Name = request.Name;
		existSource.Address = request.Address;
		existSource.Type = request.Type;
		existSource.Description = request.Description;
		existSource.IsDisabled = request.IsDisabled;

		await db.SaveChangesAsync();

		return new()
		{
			UpdatedEntities = [existSource]
		};
	}

	public async Task<DatabaseResult<SourceEntity>> DeleteAsync(int sourceId)
	{
		var existSource = await db.Sources.FirstOrDefaultAsync(x => x.Id == sourceId)
			?? throw new NotFoundException($"Источник #{sourceId} не найден");

		existSource.IsDeleted = true;

		await db.SaveChangesAsync();

		return new()
		{
			UpdatedEntities = [existSource]
		};
	}
}
