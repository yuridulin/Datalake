using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.UserGroups.Commands.DeleteUserGroup;

public interface IDeleteUserGroupHandler : ICommandHandler<DeleteUserGroupCommand, bool> { }

public class DeleteUserGroupHandler(
	IUserGroupsRepository userGroupsRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache,
	ILogger<DeleteUserGroupCommand> logger) :
		TransactionalCommandHandler<DeleteUserGroupCommand, bool>(unitOfWork, logger, inventoryCache),
		IDeleteUserGroupHandler
{
	public override void CheckPermissions(DeleteUserGroupCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Manager);
	}

	UserGroupEntity userGroup = null!;

	public override async Task<bool> ExecuteInTransactionAsync(DeleteUserGroupCommand command, CancellationToken ct = default)
	{
		userGroup = await userGroupsRepository.GetByIdAsync(command.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUserGroup(command.Guid);

		userGroup.MarkAsDeleted();

		await userGroupsRepository.UpdateAsync(userGroup, ct);
		await auditRepository.AddAsync(new(command.User.Guid, "Группа учетных записей удалена", userGroupGuid: userGroup.Guid), ct);

		return true;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithUserGroup(userGroup);
}
