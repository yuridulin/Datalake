using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface ITagInputsRepository : IRepository<TagInputEntity, int>
{
	Task<IEnumerable<TagInputEntity>> GetByTagIdAsync(int tagId, CancellationToken ct = default);
}
