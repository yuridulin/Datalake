using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Domain.Entities;

namespace Datalake.InventoryService.Application.Repositories;

public interface ITagInputsRepository : IRepository<TagInputEntity, int>
{
	Task<IEnumerable<TagInputEntity>> GetByTagIdAsync(int tagId, CancellationToken ct = default);
}
