using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;

namespace Datalake.Inventory.Application.Features.Cache.Commands.ReloadCache;

public interface IReloadCacheHandler : ICommandHandler<ReloadCacheCommand, bool> { }

public class ReloadCacheHandler(IInventoryCache cache) : IReloadCacheHandler
{
	public async Task<bool> HandleAsync(ReloadCacheCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		await cache.RestoreAsync();

		return true;
	}
}
