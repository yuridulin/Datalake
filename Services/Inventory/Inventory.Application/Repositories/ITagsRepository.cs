using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface ITagsRepository : IRepository<Tag, int>
{
	Task<bool> ExistsRangeAsync(IEnumerable<int> identifiers, CancellationToken ct = default);
}
