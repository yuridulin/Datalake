using Datalake.Domain.Entities;
using Datalake.Domain.ValueObjects;
using Datalake.Gateway.Application.Interfaces;
using Datalake.Gateway.Application.Interfaces.Repositories;
using Datalake.Shared.Application.Exceptions;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Commands.OpenLocalSession;

public interface IOpenLocalSessionHandler : ICommandHandler<OpenLocalSessionCommand, string> { }

public class OpenLocalSessionHandler(
	IUnitOfWork unitOfWork,
	IUsersRepository usersRepository,
	ISessionsService sessionsService) : TransactionHandler<OpenLocalSessionCommand, string>(unitOfWork), IOpenLocalSessionHandler
{
	public override async Task<string> HandleInTransactionAsync(OpenLocalSessionCommand command, CancellationToken ct = default)
	{
		User? user = await usersRepository.GetByLoginAsync(command.Login, ct)
			?? throw new NotFoundException("Пользователь не найден по логину");

		var passwordHash = PasswordHashValue.FromPlainText(command.PasswordString);
		if (user.PasswordHash != passwordHash)
			throw new UnauthenticatedException("Пароль не подходит");

		var token = await sessionsService.OpenAsync(user, ct);
		return token;
	}
}
