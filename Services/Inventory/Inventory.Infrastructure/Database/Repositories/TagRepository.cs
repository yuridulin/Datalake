using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class TagRepository(InventoryDbContext context) : DbRepository<Tag, int>(context), ITagsRepository
{
	public override Task<Tag?> GetByIdAsync(int id, CancellationToken ct = default)
	{
		return _set.FirstOrDefaultAsync(x => x.Id == id, cancellationToken: ct);
	}

	public override Task<bool> ExistsAsync(int id, CancellationToken ct = default)
	{
		return _set.AnyAsync(x => x.Id == id && !x.IsDeleted, cancellationToken: ct);
	}

	public Task<bool> ExistsRangeAsync(IEnumerable<int> identifiers, CancellationToken ct = default)
	{
		return _set.AnyAsync(x => identifiers.Contains(x.Id) && !x.IsDeleted, cancellationToken: ct);
	}
}
