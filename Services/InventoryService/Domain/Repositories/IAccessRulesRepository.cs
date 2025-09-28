using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Domain.Repositories;

public interface IAccessRulesRepository : IRepository<AccessRuleEntity, int>
{
	public Task<IEnumerable<AccessRuleEntity>> GetBlockRulesAsync(int blockId);

	public Task<IEnumerable<AccessRuleEntity>> GetSourceRulesAsync(int sourceId);

	public Task<IEnumerable<AccessRuleEntity>> GetTagRulesAsync(int tagId);

	public Task<IEnumerable<AccessRuleEntity>> GetUserRulesAsync(Guid userGuid);

	public Task<IEnumerable<AccessRuleEntity>> GetUserGroupRulesAsync(Guid userGroupGuid);
}
