using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Features.UserGroups.Commands.MoveUserGroup;

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
		command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Manager);
	}

	UserGroupEntity userGroup = null!;

	public override async Task<bool> ExecuteInTransactionAsync(MoveUserGroupCommand command, CancellationToken ct = default)
	{
		userGroup = await userGroupsRepository.GetByIdAsync(command.Guid, ct)
			?? throw Errors.NotFoundUserGroup(command.Guid);

		userGroup.UpdateParent(command.ParentGuid);

		await userGroupsRepository.UpdateAsync(userGroup, ct);
		await auditRepository.AddAsync(new(command.User.Guid, "Группа учетных записей перемещена", userGroupGuid: userGroup.Guid), ct);

		return true;
	}

	public override InventoryState UpdateCache(InventoryState state) => state.WithUserGroup(userGroup);
}
