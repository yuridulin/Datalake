using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class UserGroupRelationsRepository(InventoryEfContext context) : EfRepository<UserGroupRelationEntity, int>(context), IUserGroupRelationsRepository
{
	public async Task<UserGroupRelationEntity[]> GetByUserGroupGuidAsync(Guid userGroupGuid, CancellationToken ct = default)
	{
		return await _set.Where(x => x.UserGroupGuid == userGroupGuid).ToArrayAsync(ct);
	}
}
