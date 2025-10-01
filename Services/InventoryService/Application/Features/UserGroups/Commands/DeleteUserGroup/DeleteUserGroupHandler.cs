using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Features.UserGroups.Commands.DeleteUserGroup;

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
		command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Manager);
	}

	UserGroupEntity userGroup = null!;

	public override async Task<bool> ExecuteInTransactionAsync(DeleteUserGroupCommand command, CancellationToken ct = default)
	{
		userGroup = await userGroupsRepository.GetByIdAsync(command.Guid, ct)
			?? throw Errors.NotFoundUserGroup(command.Guid);

		userGroup.MarkAsDeleted();

		await userGroupsRepository.UpdateAsync(userGroup, ct);
		await auditRepository.AddAsync(new(command.User.Guid, "Группа учетных записей удалена", userGroupGuid: userGroup.Guid), ct);

		return true;
	}

	public override InventoryState UpdateCache(InventoryState state) => state.WithUserGroup(userGroup);
}
