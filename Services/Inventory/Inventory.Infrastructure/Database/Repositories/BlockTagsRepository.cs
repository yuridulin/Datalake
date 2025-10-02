using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Domain.Entities;
using Datalake.Inventory.Infrastructure.Database.Abstractions;
using Datalake.Shared.Application.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.Inventory.Infrastructure.Database.Repositories;

[Scoped]
public class BlockTagsRepository(InventoryEfContext context) : EfRepository<BlockTagEntity, int>(context), IBlockTagsRepository
{
	public async Task<IEnumerable<BlockTagEntity>> GetByBlockIdAsync(int blockId, CancellationToken ct = default)
	{
		return await _set.Where(x => x.BlockId == blockId).ToArrayAsync(ct);
	}
}
