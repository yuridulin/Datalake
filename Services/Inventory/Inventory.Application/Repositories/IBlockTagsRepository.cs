using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces.Persistent;

namespace Datalake.Inventory.Application.Repositories;

public interface IBlockTagsRepository : IRepository<BlockTag, int>
{
	Task<IEnumerable<BlockTag>> GetByBlockIdAsync(int blockId, CancellationToken ct = default);
}
