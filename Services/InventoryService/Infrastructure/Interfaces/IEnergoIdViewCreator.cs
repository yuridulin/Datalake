namespace Datalake.InventoryService.Infrastructure.Interfaces;

public interface IEnergoIdViewCreator
{
	Task RecreateAsync(CancellationToken ct = default);
}