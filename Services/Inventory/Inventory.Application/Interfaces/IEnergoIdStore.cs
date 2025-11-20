using Microsoft.Extensions.Hosting;

namespace Datalake.Inventory.Application.Interfaces;

public interface IEnergoIdStore : IHostedService
{
	IEnergoIdState State { get; }

	void SetReady();

	Task UpdateAsync(CancellationToken ct = default);
}