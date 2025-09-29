using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Datalake.InventoryService.Infrastructure.Database.Repositories;

[Scoped]
public class BlockTagsRepository(InventoryEfContext context) : EfRepository<BlockTagEntity, int>(context), IBlockTagsRepository
{
	public async Task<IEnumerable<BlockTagEntity>> GetByBlockIdAsync(int blockId)
	{
		return await _set.Where(x => x.BlockId == blockId).ToArrayAsync();
	}
}
