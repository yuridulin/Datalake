using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Domain.Entities;

namespace Datalake.InventoryService.Application.Repositories;

public interface IBlockTagsRepository : IRepository<BlockTagEntity, int>
{
	Task<IEnumerable<BlockTagEntity>> GetByBlockIdAsync(int blockId, CancellationToken ct = default);
}
