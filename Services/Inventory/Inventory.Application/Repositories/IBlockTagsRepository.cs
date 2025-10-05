using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface IBlockTagsRepository : IRepository<BlockTag, int>
{
	Task<IEnumerable<BlockTag>> GetByBlockIdAsync(int blockId, CancellationToken ct = default);
}
