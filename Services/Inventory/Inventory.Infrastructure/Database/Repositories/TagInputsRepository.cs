using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class TagInputsRepository(InventoryEfContext context) : EfRepository<TagInputEntity, int>(context), ITagInputsRepository
{
	public async Task<IEnumerable<TagInputEntity>> GetByTagIdAsync(int tagId, CancellationToken ct = default)
	{
		return await _set.Where(x => x.TagId == tagId).ToArrayAsync(ct);
	}
}
