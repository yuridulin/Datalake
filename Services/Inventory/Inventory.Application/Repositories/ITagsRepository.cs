using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface ITagsRepository : IRepository<TagEntity, int>
{
	Task<bool> ExistsRangeAsync(IEnumerable<int> identifiers, CancellationToken ct = default);
}
