using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class SourcesRepository(InventoryEfContext context) : EfRepository<SourceEntity, int>(context), ISourcesRepository
{
	public override Task<SourceEntity?> GetByIdAsync(int id, CancellationToken ct = default)
	{
		return _set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: ct);
	}

	public override Task<bool> ExistsAsync(int id, CancellationToken ct = default)
	{
		return _set.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken: ct);
	}
}
