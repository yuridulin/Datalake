using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Domain.Entities;

namespace Datalake.InventoryService.Application.Repositories;

public interface IUserGroupRelationsRepository : IRepository<UserGroupRelationEntity, int>
{
	Task<UserGroupRelationEntity[]> GetByUserGroupGuidAsync(Guid userGroupGuid, CancellationToken ct = default);
}
