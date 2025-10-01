using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Features.Users.Commands.CreateUser;

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
		command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Admin);
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

	public override InventoryState UpdateCache(InventoryState state) => state.WithUser(user);
}
