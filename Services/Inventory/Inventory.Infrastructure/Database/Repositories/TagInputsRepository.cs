using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class TagInputsRepository(InventoryDbContext context) : DbRepository<TagInput, int>(context), ITagInputsRepository
{
	public async Task<IEnumerable<TagInput>> GetByTagIdAsync(int tagId, CancellationToken ct = default)
	{
		return await _set.Where(x => x.TagId == tagId).ToArrayAsync(ct);
	}
}
