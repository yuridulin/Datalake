using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class AccessRulesRepository(InventoryEfContext context) : EfRepository<AccessRuleEntity, int>(context), IAccessRulesRepository
{
	public async Task<IEnumerable<AccessRuleEntity>> GetBlockRulesAsync(int blockId)
	{
		return await _set.Where(x => x.BlockId == blockId && !x.IsGlobal).ToArrayAsync();
	}

	public async Task<IEnumerable<AccessRuleEntity>> GetSourceRulesAsync(int sourceId)
	{
		return await _set.Where(x => x.SourceId == sourceId && !x.IsGlobal).ToArrayAsync();
	}

	public async Task<IEnumerable<AccessRuleEntity>> GetTagRulesAsync(int tagId)
	{
		return await _set.Where(x => x.TagId == tagId && !x.IsGlobal).ToArrayAsync();
	}

	public async Task<IEnumerable<AccessRuleEntity>> GetUserGroupRulesAsync(Guid userGroupGuid)
	{
		return await _set.Where(x => x.UserGroupGuid == userGroupGuid && !x.IsGlobal).ToArrayAsync();
	}

	public async Task<IEnumerable<AccessRuleEntity>> GetUserRulesAsync(Guid userGuid)
	{
		return await _set.Where(x => x.UserGuid == userGuid && !x.IsGlobal).ToArrayAsync();
	}
}
