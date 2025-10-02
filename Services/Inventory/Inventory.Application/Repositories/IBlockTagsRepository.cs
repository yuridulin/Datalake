using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface IBlockTagsRepository : IRepository<BlockTagEntity, int>
{
	Task<IEnumerable<BlockTagEntity>> GetByBlockIdAsync(int blockId, CancellationToken ct = default);
}
