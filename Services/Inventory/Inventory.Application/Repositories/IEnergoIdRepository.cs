using Datalake.Domain.Entities;

namespace Datalake.Inventory.Application.Repositories;

public interface IEnergoIdRepository
{
	Task<IEnumerable<EnergoId>> GetAsync(CancellationToken ct = default);
}
