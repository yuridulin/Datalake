using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.UserGroups.Commands.UpdateUserGroup;

public interface IUpdateUserGroupHandler : ICommandHandler<UpdateUserGroupCommand, bool> { }

public class UpdateUserGroupHandler(
	IUserGroupsRepository userGroupsRepository,
	IUserGroupRelationsRepository userGroupRelationsRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache,
	ILogger<UpdateUserGroupCommand> logger) :
		TransactionalCommandHandler<UpdateUserGroupCommand, bool>(unitOfWork, logger, inventoryCache),
		IUpdateUserGroupHandler
{
	public override void CheckPermissions(UpdateUserGroupCommand command)
	{
		command.User.ThrowIfNoAccessToUserGroup(AccessType.Manager, command.Guid);
	}

	UserGroup userGroup = null!;
	UserGroupRelation[] userGroupRelations = null!;

	public override async Task<bool> ExecuteInTransactionAsync(UpdateUserGroupCommand command, CancellationToken ct = default)
	{
		userGroup = await userGroupsRepository.GetByIdAsync(command.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUserGroup(command.Guid);

		userGroup.Update(command.Name, command.Description);
		await userGroupsRepository.UpdateAsync(userGroup, ct);

		var oldUserGroupRelations = await userGroupRelationsRepository.GetByUserGroupGuidAsync(userGroup.Guid, ct);
		await userGroupRelationsRepository.RemoveRangeAsync(oldUserGroupRelations, ct);

		if (command.Users.Any())
		{
			userGroupRelations = command.Users
				.Select(x => new UserGroupRelation(
					userGroupGuid: userGroup.Guid,
					userGuid: x.Guid,
					accessType: x.AccessType))
				.ToArray();
			await userGroupRelationsRepository.AddRangeAsync(userGroupRelations, ct);
		}

		await auditRepository.AddAsync(new(command.User.Guid, "Группа учетных записей изменена: diff", userGroupGuid: userGroup.Guid), ct);

		return true;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state
		.WithUserGroup(userGroup)
		.WithUserGroupRelations(userGroup.Guid, userGroupRelations);
}
