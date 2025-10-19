using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.Users.Commands.CreateUser;

public interface ICreateUserHandler : ICommandHandler<CreateUserCommand, Guid> { }

public class CreateUserHandler(
	IUsersRepository usersRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	ILogger<CreateUserHandler> logger,
	IInventoryCache inventoryCache) :
		TransactionalCommandHandler<CreateUserCommand, Guid>(unitOfWork, logger, inventoryCache),
		ICreateUserHandler
{
	public override void CheckPermissions(CreateUserCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);
	}

	User user = null!;

	public override async Task<Guid> ExecuteInTransactionAsync(CreateUserCommand command, CancellationToken ct = default)
	{
		user = User.CreateWithType(
			type: command.Type,
			energoIdGuid: command.EnergoIdGuid,
			login: command.Login,
			passwordString: command.Password,
			fullName: command.FullName,
			email: command.Email);

		await usersRepository.AddAsync(user, ct);

		await auditRepository.AddAsync(new(command.User.Guid, message: "Создан новый пользователь: " + user.Type.ToString(), userGuid: user.Guid), ct);

		return user.Guid;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithUser(user);
}
