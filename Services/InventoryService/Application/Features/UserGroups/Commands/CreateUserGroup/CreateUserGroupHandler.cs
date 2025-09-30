using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Features.UserGroups.Commands.CreateUserGroup;

public interface ICreateUserGroupHandler : ICommandHandler<CreateUserGroupCommand, Guid> { }

public class CreateUserGroupHandler(
	IUserGroupsRepository userGroupsRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache,
	ILogger<CreateUserGroupHandler> logger) :
		TransactionalCommandHandler<CreateUserGroupCommand, Guid>(unitOfWork, logger, inventoryCache),
		ICreateUserGroupHandler
{
	public override void CheckPermissions(CreateUserGroupCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Manager);
	}

	UserGroupEntity userGroup = null!;

	public override async Task<Guid> ExecuteInTransactionAsync(CreateUserGroupCommand command, CancellationToken ct = default)
	{
		userGroup = new(parentGuid: command.ParentGuid, name: command.Name, description: command.Description);

		await userGroupsRepository.AddAsync(userGroup, ct);
		await _unitOfWork.SaveChangesAsync(ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Группа учетных записей создана", userGroupGuid: userGroup.Guid), ct);

		return userGroup.Guid;
	}

	public override InventoryState UpdateCache(InventoryState state) => state.WithUserGroup(userGroup);
}
