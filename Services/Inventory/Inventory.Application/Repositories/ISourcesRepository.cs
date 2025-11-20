using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces;

namespace Datalake.Inventory.Application.Repositories;

public interface ISourcesRepository : IRepository<Source, int>
{
	Task<IEnumerable<Source>> GetAllAsync(CancellationToken ct = default);
}
