using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class AccessRulesRepository(InventoryDbContext context) : DbRepository<AccessRule, int>(context), IAccessRulesRepository
{
	public async Task<IEnumerable<AccessRule>> GetBlockRulesAsync(int blockId, CancellationToken ct = default)
	{
		return await _set.Where(x => x.BlockId == blockId && !x.IsGlobal).ToArrayAsync(ct);
	}

	public async Task<IEnumerable<AccessRule>> GetSourceRulesAsync(int sourceId, CancellationToken ct = default)
	{
		return await _set.Where(x => x.SourceId == sourceId && !x.IsGlobal).ToArrayAsync(ct);
	}

	public async Task<IEnumerable<AccessRule>> GetTagRulesAsync(int tagId, CancellationToken ct = default)
	{
		return await _set.Where(x => x.TagId == tagId && !x.IsGlobal).ToArrayAsync(ct);
	}

	public async Task<IEnumerable<AccessRule>> GetUserGroupRulesAsync(Guid userGroupGuid, CancellationToken ct = default)
	{
		return await _set.Where(x => x.UserGroupGuid == userGroupGuid && !x.IsGlobal).ToArrayAsync(ct);
	}

	public async Task<IEnumerable<AccessRule>> GetUserRulesAsync(Guid userGuid, CancellationToken ct = default)
	{
		return await _set.Where(x => x.UserGuid == userGuid && !x.IsGlobal).ToArrayAsync(ct);
	}

	public async Task<AccessRule?> GetUserGlobalRuleAsync(Guid userGuid, CancellationToken ct = default)
	{
		return await _set.FirstOrDefaultAsync(x => x.UserGuid.Equals(userGuid) && x.IsGlobal, cancellationToken: ct);
	}

	public async Task<AccessRule?> GetUserGroupGlobalRuleAsync(Guid userGroupGuid, CancellationToken ct = default)
	{
		return await _set.FirstOrDefaultAsync(x => x.UserGroupGuid.Equals(userGroupGuid) && x.IsGlobal, cancellationToken: ct);
	}
}
