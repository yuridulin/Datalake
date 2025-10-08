using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces.Persistent;

namespace Datalake.Inventory.Application.Repositories;

public interface ITagInputsRepository : IRepository<TagInput, int>
{
	Task<IEnumerable<TagInput>> GetByTagIdAsync(int tagId, CancellationToken ct = default);
}
