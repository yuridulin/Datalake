using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Domain.Repositories;

public interface IUserGroupRelationsRepository : IRepository<UserGroupRelationEntity, int>
{
	Task<UserGroupRelationEntity[]> GetByUserGroupGuidAsync(Guid userGroupGuid, CancellationToken ct = default);
}
