using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.UserGroups.Commands.CreateUserGroup;

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
		command.User.ThrowIfNoGlobalAccess(AccessType.Manager);
	}

	UserGroup userGroup = null!;

	public override async Task<Guid> ExecuteInTransactionAsync(CreateUserGroupCommand command, CancellationToken ct = default)
	{
		userGroup = new(parentGuid: command.ParentGuid, name: command.Name, description: command.Description);

		await userGroupsRepository.AddAsync(userGroup, ct);
		await _unitOfWork.SaveChangesAsync(ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Группа учетных записей создана", userGroupGuid: userGroup.Guid), ct);

		return userGroup.Guid;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithUserGroup(userGroup);
}
