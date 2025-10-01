using Datalake.InventoryService.Infrastructure.Cache.EnergoId;

namespace Datalake.InventoryService.Application.Interfaces.InMemory;

public interface IEnergoIdCache
{
	EnergoIdState State { get; }

	Task UpdateAsync(CancellationToken ct = default);
}