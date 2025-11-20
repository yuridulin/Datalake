using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Cache.Commands.ReloadCache;

public interface IReloadCacheHandler : ICommandHandler<ReloadCacheCommand, bool> { }

public class ReloadCacheHandler(IInventoryStore cache) : IReloadCacheHandler
{
	public async Task<bool> HandleAsync(ReloadCacheCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		await cache.RestoreAsync();

		return true;
	}
}
