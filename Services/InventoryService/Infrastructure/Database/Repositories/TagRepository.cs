using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class TagRepository(InventoryEfContext context) : EfRepository<TagEntity, int>(context), ITagsRepository
{
	public override Task<TagEntity?> GetByIdAsync(int id, CancellationToken ct = default)
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
