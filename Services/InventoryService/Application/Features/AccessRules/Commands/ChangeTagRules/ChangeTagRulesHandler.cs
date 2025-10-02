using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeTagRules;

public interface IChangeTagRulesHandler : ICommandHandler<ChangeTagRulesCommand, bool> { }

public class ChangeTagRulesHandler(
	ITagsRepository tagsRepository,
	IAccessRulesRepository accessRulesRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache) : IChangeTagRulesHandler
{
	public async Task<bool> HandleAsync(ChangeTagRulesCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		TagEntity tag;
		int[] oldRulesId;
		AccessRuleEntity[] newRules;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			tag = await tagsRepository.GetByIdAsync(command.TagId, ct)
				?? throw Errors.NotFoundTag(command.TagId);

			var oldRules = await accessRulesRepository.GetTagRulesAsync(tag.Id);
			oldRulesId = oldRules.Select(x => x.Id).ToArray();
			await accessRulesRepository.RemoveRangeAsync(oldRules, ct);

			newRules = command.Rules.Select(x => new AccessRuleEntity(x.Type, tagId: tag.Id, userGuid: x.UserGuid, userGroupGuid: x.UserGroupGuid)).ToArray();
			await accessRulesRepository.AddRangeAsync(newRules, ct);

			var audit = new AuditEntity(command.User.Guid, "Изменены права доступа", tagId: tag.Id);
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
