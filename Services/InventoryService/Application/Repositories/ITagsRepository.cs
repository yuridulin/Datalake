using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Domain.Entities;

namespace Datalake.InventoryService.Application.Repositories;

public interface ITagsRepository : IRepository<TagEntity, int>
{
	Task<bool> ExistsRangeAsync(IEnumerable<int> identifiers, CancellationToken ct = default);
}
