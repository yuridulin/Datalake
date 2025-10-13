using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class SourcesRepository(InventoryDbContext context) : DbRepository<Source, int>(context), ISourcesRepository
{
	public override async Task<Source?> GetByIdAsync(int id, CancellationToken ct = default)
	{
		return await _set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: ct);
	}

	public override async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
	{
		return await _set.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken: ct);
	}

	public async Task<IEnumerable<Source>> GetAllAsync(CancellationToken ct = default)
	{
		return await _set.ToArrayAsync(cancellationToken: ct);
	}
}
