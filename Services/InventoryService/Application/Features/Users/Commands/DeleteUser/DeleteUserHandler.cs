using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Features.Users.Commands.DeleteUser;

public interface IDeleteUserHandler : ICommandHandler<DeleteUserCommand, bool> { }

public class DeleteUserHandler(
	IUsersRepository usersRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	ILogger<DeleteUserHandler> logger,
	IInventoryCache inventoryCache) :
		TransactionalCommandHandler<DeleteUserCommand, bool>(unitOfWork, logger, inventoryCache),
		IDeleteUserHandler
{
	public override void CheckPermissions(DeleteUserCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Admin);
	}

	UserEntity user = null!;

	public override async Task<bool> ExecuteInTransactionAsync(DeleteUserCommand command, CancellationToken ct = default)
	{
		user = await usersRepository.GetByIdAsync(command.Guid, ct)
			?? throw Errors.NotFoundUser(command.Guid);

		user.MarkAsDeleted();

		await usersRepository.UpdateAsync(user, ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Учетная запись удалена", userGuid: user.Guid), ct);

		return true;
	}

	public override InventoryState UpdateCache(InventoryState state) => state.WithUser(user);
}
