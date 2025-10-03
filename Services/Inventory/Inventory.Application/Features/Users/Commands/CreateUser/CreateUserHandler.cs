using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
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

	UserEntity user = null!;

	public override async Task<Guid> ExecuteInTransactionAsync(CreateUserCommand command, CancellationToken ct = default)
	{
		user = new(
			type: command.Type,
			fullName: command.FullName,
			login: command.Login,
			passwordString: command.Password,
			energoIdGuid: command.EnergoIdGuid,
			host: command.StaticHost);

		await usersRepository.AddAsync(user, ct);

		await auditRepository.AddAsync(new(command.User.Guid, message: "Создан новый пользователь: " + user.Type.ToString(), userGuid: user.Guid), ct);

		return user.Guid;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithUser(user);
}
