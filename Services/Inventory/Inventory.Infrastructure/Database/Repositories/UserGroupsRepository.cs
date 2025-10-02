using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Domain.Entities;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class UserGroupsRepository(InventoryEfContext context) : EfRepository<UserGroupEntity, Guid>(context), IUserGroupsRepository
{
	public override Task<UserGroupEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		return _set.FirstOrDefaultAsync(x => x.Guid == id, cancellationToken: ct);
	}

	public override Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
	{
		return _set.AnyAsync(x => x.Guid == id && !x.IsDeleted, cancellationToken: ct);
	}
}
