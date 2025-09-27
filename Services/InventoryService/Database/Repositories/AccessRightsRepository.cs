using Datalake.InventoryService.Database.Models;
using Datalake.InventoryService.Database.Tables;
using Datalake.PrivateApi.Attributes;

namespace Datalake.InventoryService.Database.Repositories;

/// <summary>
/// Репозиторий работы с правами доступа
/// </summary>
[Scoped]
public class AccessRightsRepository(InventoryEfContext db)
{
	public async Task<DatabaseResult<AccessRights>> ReplaceUserGroupRightsAsync(Guid userGroupGuid, IEnumerable<AccessRights> newRights)
	{
		var existingRights = db.AccessRights.Where(x => !x.IsGlobal && x.UserGroupGuid == userGroupGuid).ToArray();

		db.RemoveRange(existingRights);
		db.AddRange(newRights);

		await db.SaveChangesAsync();

		var result = new DatabaseResult<AccessRights>
		{
			DeletedIdentifiers = existingRights.Select(x => x.Id).ToArray(),
			AddedEntities = newRights.ToArray(),
		};

		return result;
	}

	public async Task<DatabaseResult<AccessRights>> ReplaceUserRightsAsync(Guid userGuid, IEnumerable<AccessRights> newRights)
	{
		var existingRights = db.AccessRights.Where(x => !x.IsGlobal && x.UserGuid == userGuid).ToArray();

		db.RemoveRange(existingRights);
		db.AddRange(newRights);

		await db.SaveChangesAsync();

		var result = new DatabaseResult<AccessRights>
		{
			DeletedIdentifiers = existingRights.Select(x => x.Id).ToArray(),
			AddedEntities = newRights.ToArray(),
		};

		return result;
	}

	public async Task<DatabaseResult<AccessRights>> ReplaceSourceRightsAsync(int sourceId, IEnumerable<AccessRights> newRights)
	{
		var existingRights = db.AccessRights.Where(x => !x.IsGlobal && x.SourceId == sourceId).ToArray();

		db.RemoveRange(existingRights);
		db.AddRange(newRights);

		await db.SaveChangesAsync();

		var result = new DatabaseResult<AccessRights>
		{
			DeletedIdentifiers = existingRights.Select(x => x.Id).ToArray(),
			AddedEntities = newRights.ToArray(),
		};

		return result;
	}

	public async Task<DatabaseResult<AccessRights>> ReplaceBlockRightsAsync(int blockId, IEnumerable<AccessRights> newRights)
	{
		var existingRights = db.AccessRights.Where(x => !x.IsGlobal && x.BlockId == blockId).ToArray();

		db.RemoveRange(existingRights);
		db.AddRange(newRights);

		await db.SaveChangesAsync();

		var result = new DatabaseResult<AccessRights>
		{
			DeletedIdentifiers = existingRights.Select(x => x.Id).ToArray(),
			AddedEntities = newRights.ToArray(),
		};

		return result;
	}

	public async Task<DatabaseResult<AccessRights>> ReplaceTagRightsAsync(int tagId, IEnumerable<AccessRights> newRights)
	{
		var existingRights = db.AccessRights.Where(x => !x.IsGlobal && x.TagId == tagId).ToArray();

		db.RemoveRange(existingRights);
		db.AddRange(newRights);

		await db.SaveChangesAsync();

		var result = new DatabaseResult<AccessRights>
		{
			DeletedIdentifiers = existingRights.Select(x => x.Id).ToArray(),
			AddedEntities = newRights.ToArray(),
		};

		return result;
	}
}
