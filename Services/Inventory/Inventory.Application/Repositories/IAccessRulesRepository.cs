using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces;

namespace Datalake.Inventory.Application.Repositories;

public interface IAccessRulesRepository : IRepository<AccessRule, int>
{
	public Task<IEnumerable<AccessRule>> GetBlockRulesAsync(int blockId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRule>> GetSourceRulesAsync(int sourceId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRule>> GetTagRulesAsync(int tagId, CancellationToken ct = default);

	public Task<IEnumerable<AccessRule>> GetUserRulesAsync(Guid userGuid, CancellationToken ct = default);

	public Task<IEnumerable<AccessRule>> GetUserGroupRulesAsync(Guid userGroupGuid, CancellationToken ct = default);

	public Task<AccessRule?> GetUserGlobalRuleAsync(Guid userGuid, CancellationToken ct = default);

	public Task<AccessRule?> GetUserGroupGlobalRuleAsync(Guid userGroupGuid, CancellationToken ct = default);
}
