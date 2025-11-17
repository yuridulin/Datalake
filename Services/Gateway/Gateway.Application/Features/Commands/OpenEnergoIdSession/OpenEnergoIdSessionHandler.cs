using Datalake.Domain.Entities;
using Datalake.Gateway.Application.Interfaces;
using Datalake.Gateway.Application.Interfaces.Repositories;
using Datalake.Shared.Application.Exceptions;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Commands.OpenEnergoIdSession;

public interface IOpenEnergoIdSessionHandler : ICommandHandler<OpenEnergoIdSessionCommand, string> { }

public class OpenEnergoIdSessionHandler(
	IUnitOfWork unitOfWork,
	IUsersRepository usersRepository,
	ISessionsService sessionsService) : TransactionHandler<OpenEnergoIdSessionCommand, string>(unitOfWork), IOpenEnergoIdSessionHandler
{
	public override async Task<string> HandleInTransactionAsync(OpenEnergoIdSessionCommand command, CancellationToken ct = default)
	{
		User? user = await usersRepository.GetByGuidAsync(command.Guid, ct)
			?? throw new NotFoundException("Пользователь не найден по идентификатору EnergoId");

		var token = await sessionsService.OpenAsync(user, ct);
		return token;
	}
}
