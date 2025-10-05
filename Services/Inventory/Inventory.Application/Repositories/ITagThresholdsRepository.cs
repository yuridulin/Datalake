using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface ITagThresholdsRepository : IRepository<TagThreshold, int>
{
	Task<IEnumerable<TagThreshold>> GetByTagIdAsync(int tagId, CancellationToken ct = default);
}
