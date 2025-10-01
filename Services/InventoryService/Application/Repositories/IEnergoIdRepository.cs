using Datalake.InventoryService.Domain.Entities;

namespace Datalake.InventoryService.Application.Repositories;

public interface IEnergoIdRepository
{
	Task<IEnumerable<EnergoIdEntity>> GetAsync(CancellationToken ct = default);
}
