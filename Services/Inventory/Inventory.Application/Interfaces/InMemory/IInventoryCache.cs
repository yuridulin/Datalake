namespace Datalake.Inventory.Application.Interfaces.InMemory;

public interface IInventoryCache
{
	IInventoryCacheState State { get; }

	Task<IInventoryCacheState> UpdateAsync(Func<IInventoryCacheState, IInventoryCacheState> update);

	Task RestoreAsync();

	event EventHandler<IInventoryCacheState>? StateChanged;
}
