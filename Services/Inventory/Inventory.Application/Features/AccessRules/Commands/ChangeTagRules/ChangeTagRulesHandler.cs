using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeTagRules;

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

		Tag tag;
		int[] oldRulesId;
		AccessRule[] newRules;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			tag = await tagsRepository.GetByIdAsync(command.TagId, ct)
				?? throw InventoryNotFoundException.NotFoundTag(command.TagId);

			var oldRules = await accessRulesRepository.GetTagRulesAsync(tag.Id);
			oldRulesId = oldRules.Select(x => x.Id).ToArray();
			await accessRulesRepository.RemoveRangeAsync(oldRules, ct);

			newRules = command.Rules.Select(x => new AccessRule(x.Type, tagId: tag.Id, userGuid: x.UserGuid, userGroupGuid: x.UserGroupGuid)).ToArray();
			await accessRulesRepository.AddRangeAsync(newRules, ct);

			var audit = new AuditLog(command.User.Guid, "Изменены права доступа", tagId: tag.Id);
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
