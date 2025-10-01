using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;

namespace Datalake.InventoryService.Application.Features.EnergoId.Commands.ReloadEnergoId;

public interface IReloadEnergoIdHandler : ICommandHandler<ReloadEnergoIdCommand, bool> { }

public class ReloadEnergoIdHandler(IEnergoIdCache energoIdCache) : IReloadEnergoIdHandler
{
	public async Task<bool> HandleAsync(ReloadEnergoIdCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Admin);

		await energoIdCache.UpdateAsync(ct);

		return true;
	}
}
