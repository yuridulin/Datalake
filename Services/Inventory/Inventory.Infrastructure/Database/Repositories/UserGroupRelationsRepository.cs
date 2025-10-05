using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class UserGroupRelationsRepository(InventoryDbContext context) : DbRepository<UserGroupRelation, int>(context), IUserGroupRelationsRepository
{
	public async Task<UserGroupRelation[]> GetByUserGroupGuidAsync(Guid userGroupGuid, CancellationToken ct = default)
	{
		return await _set.Where(x => x.UserGroupGuid == userGroupGuid).ToArrayAsync(ct);
	}
}
