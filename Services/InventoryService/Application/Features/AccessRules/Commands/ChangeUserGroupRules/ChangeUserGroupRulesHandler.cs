using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeUserGroupRules;

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

		UserGroupEntity userGroup;
		int[] oldRulesId;
		AccessRuleEntity[] newRules;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			userGroup = await userGroupsRepository.GetByIdAsync(command.UserGroupGuid, ct)
				?? throw Errors.NotFoundUserGroup(command.UserGroupGuid);

			var oldRules = await accessRulesRepository.GetUserGroupRulesAsync(userGroup.Guid);
			oldRulesId = oldRules.Select(x => x.Id).ToArray();
			await accessRulesRepository.RemoveRangeAsync(oldRules, ct);

			newRules = command.Rules.Select(x => new AccessRuleEntity(x.Type, userGroupGuid: userGroup.Guid, tagId: x.TagId, sourceId: x.SourceId, blockId: x.BlockId)).ToArray();
			await accessRulesRepository.AddRangeAsync(newRules, ct);

			var audit = new AuditEntity(command.User.Guid, "Изменены права доступа", userGroupGuid: userGroup.Guid);
			await auditRepository.AddAsync(audit, ct);
			await unitOfWork.SaveChangesAsync(ct);
		}
		catch
		{
			await unitOfWork.RollbackAsync(ct);
			throw;
		}

		await inventoryCache.UpdateAsync(state => state with
		{
			AccessRules = state.AccessRules
				.RemoveAll(x => oldRulesId.Contains(x.Id))
				.AddRange(newRules)
		});

		return true;
	}
}
