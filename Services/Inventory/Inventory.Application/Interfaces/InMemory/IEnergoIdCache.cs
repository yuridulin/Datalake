namespace Datalake.Inventory.Application.Interfaces.InMemory;

public interface IEnergoIdCache
{
	IEnergoIdCacheState State { get; }

	Task UpdateAsync(CancellationToken ct = default);
}