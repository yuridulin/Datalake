using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.Users.Commands.DeleteUser;

public interface IDeleteUserHandler : ICommandHandler<DeleteUserCommand, bool> { }

public class DeleteUserHandler(
	IUsersRepository usersRepository,
	IAuditRepository auditRepository,
	ICalculatedAccessRulesRepository calculatedAccessRulesRepository,
	IUnitOfWork unitOfWork,
	ILogger<DeleteUserHandler> logger,
	IInventoryCache inventoryCache) :
		TransactionalCommandHandler<DeleteUserCommand, bool>(unitOfWork, logger, inventoryCache),
		IDeleteUserHandler
{
	public override void CheckPermissions(DeleteUserCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);
	}

	User user = null!;

	public override async Task<bool> ExecuteInTransactionAsync(DeleteUserCommand command, CancellationToken ct = default)
	{
		user = await usersRepository.GetByIdAsync(command.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUser(command.Guid);

		user.MarkAsDeleted();

		await usersRepository.UpdateAsync(user, ct);

		await calculatedAccessRulesRepository.RemoveByUserGuid(user.Guid, ct);
		await auditRepository.AddAsync(new(command.User.Guid, "Учетная запись удалена", userGuid: user.Guid), ct);

		return true;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithUser(user);
}
