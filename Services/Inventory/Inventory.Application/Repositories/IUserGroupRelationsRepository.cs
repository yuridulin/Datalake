using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces;

namespace Datalake.Inventory.Application.Repositories;

public interface IUserGroupRelationsRepository : IRepository<UserGroupRelation, int>
{
	Task<UserGroupRelation[]> GetByUserGroupGuidAsync(Guid userGroupGuid, CancellationToken ct = default);
}
