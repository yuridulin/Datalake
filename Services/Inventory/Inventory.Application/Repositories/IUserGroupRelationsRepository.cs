using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces.Persistent;

namespace Datalake.Inventory.Application.Repositories;

public interface IUserGroupRelationsRepository : IRepository<UserGroupRelation, int>
{
	Task<UserGroupRelation[]> GetByUserGroupGuidAsync(Guid userGroupGuid, CancellationToken ct = default);
}
