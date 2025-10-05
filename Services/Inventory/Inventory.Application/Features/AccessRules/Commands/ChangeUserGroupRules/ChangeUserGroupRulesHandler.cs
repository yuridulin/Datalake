using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeUserGroupRules;

public interface IChangeUserGroupRulesHandler : ICommandHandler<ChangeUserGroupRulesCommand, bool> { }

public class ChangeUserGroupRulesHandler(
	IUserGroupsRepository userGroupsRepository,
	IAccessRulesRepository accessRulesRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache) : IChangeUserGroupRulesHandler
{
	public async Task<bool> HandleAsync(ChangeUserGroupRulesCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		UserGroup userGroup;
		int[] oldRulesId;
		AccessRights[] newRules;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			userGroup = await userGroupsRepository.GetByIdAsync(command.UserGroupGuid, ct)
				?? throw InventoryNotFoundException.NotFoundUserGroup(command.UserGroupGuid);

			var oldRules = await accessRulesRepository.GetUserGroupRulesAsync(userGroup.Guid);
			oldRulesId = oldRules.Select(x => x.Id).ToArray();
			await accessRulesRepository.RemoveRangeAsync(oldRules, ct);

			newRules = command.Rules.Select(x => new AccessRights(x.Type, userGroupGuid: userGroup.Guid, tagId: x.TagId, sourceId: x.SourceId, blockId: x.BlockId)).ToArray();
			await accessRulesRepository.AddRangeAsync(newRules, ct);

			var audit = new Log(command.User.Guid, "Изменены права доступа", userGroupGuid: userGroup.Guid);
			await auditRepository.AddAsync(audit, ct);
			await unitOfWork.SaveChangesAsync(ct);
		}
		catch
		{
			await unitOfWork.RollbackAsync(ct);
			throw;
		}

		await inventoryCache.UpdateAsync(state => state.WithAccessRules(oldRulesId, newRules));

		return true;
	}
}
