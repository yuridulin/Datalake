using Datalake.Gateway.Application.Interfaces;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Commands.CloseSession;

public interface ICloseSessionHandler : ICommandHandler<CloseSessionCommand, bool> { }

public class CloseSessionHandler(
	IUnitOfWork unitOfWork,
	ISessionsService sessionsService) : TransactionHandler<CloseSessionCommand, bool>(unitOfWork), ICloseSessionHandler
{
	public override async Task<bool> HandleInTransactionAsync(CloseSessionCommand command, CancellationToken ct = default)
	{
		await sessionsService.CloseAsync(command.Token, ct);

		return true;
	}
}
