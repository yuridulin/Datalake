using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface IEnergoIdRepository
{
	Task<IEnumerable<EnergoIdEntity>> GetAsync(CancellationToken ct = default);
}
