using Datalake.InventoryService.Infrastructure.Cache.Inventory;

namespace Datalake.InventoryService.Application.Interfaces.InMemory;

public interface IInventoryCache
{
	InventoryState State { get; }

	Task<InventoryState> UpdateAsync(Func<InventoryState, InventoryState> update);

	Task RestoreAsync();
	
	event EventHandler<InventoryState>? StateChanged;
}
