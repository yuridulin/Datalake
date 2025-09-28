namespace Datalake.InventoryService.Infrastructure.Cache.Inventory;

public interface IInventoryCache
{
	InventoryState State { get; }

	Task<InventoryState> UpdateAsync(Func<InventoryState, InventoryState> update);

	Task RestoreAsync();
	
	event EventHandler<InventoryState>? StateChanged;
}
