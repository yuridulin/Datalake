using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.Users.Commands.UpdateUser;

public interface IUpdateUserHandler : ICommandHandler<UpdateUserCommand, bool> { }

public class UpdateUserHandler(
	IUsersRepository usersRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	ILogger<UpdateUserHandler> logger,
	IInventoryCache inventoryCache) :
		TransactionalCommandHandler<UpdateUserCommand, bool>(unitOfWork, logger, inventoryCache),
		IUpdateUserHandler
{
	public override void CheckPermissions(UpdateUserCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);
	}

	UserEntity user = null!;

	public override async Task<bool> ExecuteInTransactionAsync(UpdateUserCommand command, CancellationToken ct = default)
	{
		user = await usersRepository.GetByIdAsync(command.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUser(command.Guid);

		user.Update(
			type: command.Type,
			fullName: command.FullName,
			login: command.Login,
			passwordString: command.Password,
			energoIdGuid: command.EnergoIdGuid,
			host: command.StaticHost,
			generateNewHash: command.GenerateNewHash);

		await usersRepository.UpdateAsync(user, ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Учетная запись изменена: diff", userGuid: user.Guid), ct);

		return true;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithUser(user);
}
