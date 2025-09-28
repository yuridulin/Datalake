namespace Datalake.InventoryService.Infrastructure.Cache.EnergoId;

public interface IEnergoIdCache
{
	EnergoIdState State { get; }

	Task UpdateAsync(CancellationToken ct = default);
}