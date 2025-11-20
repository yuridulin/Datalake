using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces;

namespace Datalake.Inventory.Application.Repositories;

public interface ITagThresholdsRepository : IRepository<TagThreshold, int>
{
	Task<IEnumerable<TagThreshold>> GetByTagIdAsync(int tagId, CancellationToken ct = default);
}
