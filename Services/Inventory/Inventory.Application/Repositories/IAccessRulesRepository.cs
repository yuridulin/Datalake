using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface IAccessRulesRepository : IRepository<AccessRuleEntity, int>
{
	public Task<IEnumerable<AccessRuleEntity>> GetBlockRulesAsync(int blockId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRuleEntity>> GetSourceRulesAsync(int sourceId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRuleEntity>> GetTagRulesAsync(int tagId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRuleEntity>> GetUserRulesAsync(Guid userGuid, CancellationToken ct = default);

	public Task<IEnumerable<AccessRuleEntity>> GetUserGroupRulesAsync(Guid userGroupGuid, CancellationToken ct = default);
}
