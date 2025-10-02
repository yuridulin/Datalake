namespace Datalake.Inventory.Infrastructure.Interfaces;

public interface IEnergoIdViewCreator
{
	Task RecreateAsync(CancellationToken ct = default);
}