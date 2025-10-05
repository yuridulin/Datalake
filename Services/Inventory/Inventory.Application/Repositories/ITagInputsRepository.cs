using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface ITagInputsRepository : IRepository<TagInput, int>
{
	Task<IEnumerable<TagInput>> GetByTagIdAsync(int tagId, CancellationToken ct = default);
}
