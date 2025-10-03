using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.EnergoId.Commands.ReloadEnergoId;

public interface IReloadEnergoIdHandler : ICommandHandler<ReloadEnergoIdCommand, bool> { }

public class ReloadEnergoIdHandler(IEnergoIdCache energoIdCache) : IReloadEnergoIdHandler
{
	public async Task<bool> HandleAsync(ReloadEnergoIdCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		await energoIdCache.UpdateAsync(ct);

		return true;
	}
}
