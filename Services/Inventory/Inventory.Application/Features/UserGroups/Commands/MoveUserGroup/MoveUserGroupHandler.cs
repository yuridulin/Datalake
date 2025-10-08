using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.UserGroups.Commands.MoveUserGroup;

public interface IMoveUserGroupHandler : ICommandHandler<MoveUserGroupCommand, bool> { }

public class MoveUserGroupHandler(
	IUserGroupsRepository userGroupsRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	ILogger<MoveUserGroupHandler> logger,
	IInventoryCache inventoryCache) :
		TransactionalCommandHandler<MoveUserGroupCommand, bool>(unitOfWork, logger, inventoryCache),
		IMoveUserGroupHandler
{
	public override void CheckPermissions(MoveUserGroupCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Manager);
	}

	UserGroup userGroup = null!;

	public override async Task<bool> ExecuteInTransactionAsync(MoveUserGroupCommand command, CancellationToken ct = default)
	{
		userGroup = await userGroupsRepository.GetByIdAsync(command.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUserGroup(command.Guid);

		userGroup.UpdateParent(command.ParentGuid);

		await userGroupsRepository.UpdateAsync(userGroup, ct);
		await auditRepository.AddAsync(new(command.User.Guid, "Группа учетных записей перемещена", userGroupGuid: userGroup.Guid), ct);

		return true;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithUserGroup(userGroup);
}
