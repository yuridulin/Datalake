using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Domain.Entities;

namespace Datalake.InventoryService.Application.Repositories;

public interface IAccessRulesRepository : IRepository<AccessRuleEntity, int>
{
	public Task<IEnumerable<AccessRuleEntity>> GetBlockRulesAsync(int blockId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRuleEntity>> GetSourceRulesAsync(int sourceId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRuleEntity>> GetTagRulesAsync(int tagId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRuleEntity>> GetUserRulesAsync(Guid userGuid, CancellationToken ct = default);

	public Task<IEnumerable<AccessRuleEntity>> GetUserGroupRulesAsync(Guid userGroupGuid, CancellationToken ct = default);
}
