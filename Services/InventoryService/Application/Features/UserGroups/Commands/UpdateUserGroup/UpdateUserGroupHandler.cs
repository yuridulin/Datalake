using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Features.UserGroups.Commands.UpdateUserGroup;

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
		command.User.ThrowIfNoAccessToUserGroup(PublicApi.Enums.AccessType.Manager, command.Guid);
	}

	UserGroupEntity userGroup = null!;
	UserGroupRelationEntity[] userGroupRelations = null!;

	public override async Task<bool> ExecuteInTransactionAsync(UpdateUserGroupCommand command, CancellationToken ct = default)
	{
		userGroup = await userGroupsRepository.GetByIdAsync(command.Guid, ct)
			?? throw Errors.NotFoundUserGroup(command.Guid);

		userGroup.Update(command.Name, command.Description);
		await userGroupsRepository.UpdateAsync(userGroup, ct);

		userGroupRelations = command.Users
			.Select(x => new UserGroupRelationEntity(
				userGroupGuid: userGroup.Guid,
				userGuid: x.Guid,
				accessType: x.AccessType))
			.ToArray();

		var oldUserGroupRelations = await userGroupRelationsRepository.GetByUserGroupGuidAsync(userGroup.Guid, ct);
		await userGroupRelationsRepository.RemoveRangeAsync(oldUserGroupRelations, ct);
		await userGroupRelationsRepository.AddRangeAsync(userGroupRelations, ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Группа учетных записей изменена: diff", userGroupGuid: userGroup.Guid), ct);

		return true;
	}

	public override InventoryState UpdateCache(InventoryState state) => state
		.WithUserGroup(userGroup)
		.WithUserGroupRelations(userGroup.Guid, userGroupRelations);
}
