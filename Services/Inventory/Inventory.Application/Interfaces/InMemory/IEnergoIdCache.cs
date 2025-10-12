using Microsoft.Extensions.Hosting;

namespace Datalake.Inventory.Application.Interfaces.InMemory;

public interface IEnergoIdCache : IHostedService
{
	IEnergoIdCacheState State { get; }

	void SetReady();

	Task UpdateAsync(CancellationToken ct = default);
}