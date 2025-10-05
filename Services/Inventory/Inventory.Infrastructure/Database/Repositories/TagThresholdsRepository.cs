using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class TagThresholdsRepository(InventoryDbContext context) : DbRepository<TagThreshold, int>(context), ITagThresholdsRepository
{
	public async Task<IEnumerable<TagThreshold>> GetByTagIdAsync(int tagId, CancellationToken ct = default)
	{
		return await _set.Where(x => x.TagId == tagId).ToArrayAsync(ct);
	}
}
