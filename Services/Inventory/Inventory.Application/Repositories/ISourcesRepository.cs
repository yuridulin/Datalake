using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces.Persistent;

namespace Datalake.Inventory.Application.Repositories;

public interface ISourcesRepository : IRepository<Source, int>
{
	Task<IEnumerable<Source>> GetAllAsync();
}
