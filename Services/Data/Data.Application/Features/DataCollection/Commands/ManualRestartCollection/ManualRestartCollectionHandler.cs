using Datalake.Data.Application.Features.DataCollection.Commands.RestartCollection;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.DataCollection.Commands.ManualRestartCollection;

public interface IManualRestartCollectionHandler : ICommandHandler<ManualRestartCollectionCommand, bool> { }

public class ManualRestartCollectionHandler(IRestartCollectionHandler restartCollectionHandler) : IManualRestartCollectionHandler
{
	public Task<bool> HandleAsync(ManualRestartCollectionCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(Contracts.Public.Enums.AccessType.Admin);

		return restartCollectionHandler.HandleAsync(new(), ct);
	}
}
