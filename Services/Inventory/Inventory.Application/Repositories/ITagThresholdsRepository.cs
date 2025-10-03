using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface ITagThresholdsRepository : IRepository<TagThresholdEntity, int>
{
	Task<IEnumerable<TagThresholdEntity>> GetByTagIdAsync(int tagId, CancellationToken ct = default);
}
