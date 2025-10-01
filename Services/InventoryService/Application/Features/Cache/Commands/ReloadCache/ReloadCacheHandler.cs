using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;

namespace Datalake.InventoryService.Application.Features.Cache.Commands.ReloadCache;

public interface IReloadCacheHandler : ICommandHandler<ReloadCacheCommand, bool> { }

public class ReloadCacheHandler(IInventoryCache cache) : IReloadCacheHandler
{
	public async Task<bool> HandleAsync(ReloadCacheCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Admin);

		await cache.RestoreAsync();

		return true;
	}
}
