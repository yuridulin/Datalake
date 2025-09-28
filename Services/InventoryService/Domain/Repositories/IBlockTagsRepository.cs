using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Domain.Repositories;

public interface IBlockTagsRepository : IRepository<BlockTagEntity, int>
{
	Task<IEnumerable<BlockTagEntity>> GetByBlockIdAsync(int blockId);

	Task RemoveRangeAsync(IEnumerable<BlockTagEntity> blockTags);

	Task AddRangeAsync(IEnumerable<BlockTagEntity> blockTags);
}
