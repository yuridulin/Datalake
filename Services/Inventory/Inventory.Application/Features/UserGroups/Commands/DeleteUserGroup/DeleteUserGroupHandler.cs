using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.UserGroups.Commands.DeleteUserGroup;

public interface IDeleteUserGroupHandler : ICommandHandler<DeleteUserGroupCommand, bool> { }

public class DeleteUserGroupHandler(
	IUserGroupsRepository userGroupsRepository,
	IAuditRepository auditRepository,
	ICalculatedAccessRulesRepository calculatedAccessRulesRepository,
	IUnitOfWork unitOfWork,
	IInventoryStore inventoryCache,
	ILogger<DeleteUserGroupCommand> logger) :
		TransactionalCommandHandler<DeleteUserGroupCommand, bool>(unitOfWork, logger, inventoryCache),
		IDeleteUserGroupHandler
{
	public override void CheckPermissions(DeleteUserGroupCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Manager);
	}

	UserGroup userGroup = null!;

	public override async Task<bool> ExecuteInTransactionAsync(DeleteUserGroupCommand command, CancellationToken ct = default)
	{
		userGroup = await userGroupsRepository.GetByIdAsync(command.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUserGroup(command.Guid);

		userGroup.MarkAsDeleted();

		await userGroupsRepository.UpdateAsync(userGroup, ct);

		await calculatedAccessRulesRepository.RemoveByUserGroupGuid(userGroup.Guid, ct);
		await auditRepository.AddAsync(new(command.User.Guid, "Группа учетных записей удалена", userGroupGuid: userGroup.Guid), ct);

		return true;
	}

	public override IInventoryState UpdateCache(IInventoryState state) => state.WithUserGroup(userGroup);
}
