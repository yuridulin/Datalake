using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces.Persistent;

namespace Datalake.Inventory.Application.Repositories;

public interface ITagsRepository : IRepository<Tag, int>
{
	Task<bool> ExistsRangeAsync(IEnumerable<int> identifiers, CancellationToken ct = default);
}
