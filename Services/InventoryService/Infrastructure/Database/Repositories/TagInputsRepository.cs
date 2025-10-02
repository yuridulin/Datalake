using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class TagInputsRepository(InventoryEfContext context) : EfRepository<TagInputEntity, int>(context), ITagInputsRepository
{
	public async Task<IEnumerable<TagInputEntity>> GetByTagIdAsync(int tagId, CancellationToken ct = default)
	{
		return await _set.Where(x => x.TagId == tagId).ToArrayAsync(ct);
	}
}
