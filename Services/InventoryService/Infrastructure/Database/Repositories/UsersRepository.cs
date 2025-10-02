using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class UsersRepository(InventoryEfContext context) : EfRepository<UserEntity, Guid>(context), IUsersRepository
{
	public override Task<UserEntity?> GetByIdAsync(Guid id, CancellationToken ct = default)
	{
		return _set.FirstOrDefaultAsync(x => x.Guid == id, cancellationToken: ct);
	}

	public override Task<bool> ExistsAsync(Guid id, CancellationToken ct = default)
	{
		return _set.AnyAsync(x => x.Guid == id && !x.IsDeleted, cancellationToken: ct);
	}
}
