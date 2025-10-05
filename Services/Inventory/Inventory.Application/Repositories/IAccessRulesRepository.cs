using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface IAccessRulesRepository : IRepository<AccessRights, int>
{
	public Task<IEnumerable<AccessRights>> GetBlockRulesAsync(int blockId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRights>> GetSourceRulesAsync(int sourceId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRights>> GetTagRulesAsync(int tagId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRights>> GetUserRulesAsync(Guid userGuid, CancellationToken ct = default);

	public Task<IEnumerable<AccessRights>> GetUserGroupRulesAsync(Guid userGroupGuid, CancellationToken ct = default);
}
