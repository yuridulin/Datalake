using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Domain.Entities;

namespace Datalake.InventoryService.Application.Repositories;

public interface ITagThresholdsRepository : IRepository<TagThresholdEntity, int>
{
	Task<IEnumerable<TagThresholdEntity>> GetByTagIdAsync(int tagId, CancellationToken ct = default);
}
