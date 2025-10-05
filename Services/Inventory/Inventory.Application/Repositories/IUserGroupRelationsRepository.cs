using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface IUserGroupRelationsRepository : IRepository<UserGroupRelation, int>
{
	Task<UserGroupRelation[]> GetByUserGroupGuidAsync(Guid userGroupGuid, CancellationToken ct = default);
}
