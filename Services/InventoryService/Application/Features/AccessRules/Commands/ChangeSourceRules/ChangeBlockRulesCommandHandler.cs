using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeSourceRules;

public interface IChangeSourceRulesHandler : ICommandHandler<ChangeSourceRulesCommand, bool> { }

public class ChangeSourceRulesHandler(
	ISourcesRepository sourcesRepository,
	IAccessRulesRepository accessRulesRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache) : IChangeSourceRulesHandler
{
	public async Task<bool> HandleAsync(ChangeSourceRulesCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		SourceEntity source;
		int[] oldRulesId;
		AccessRuleEntity[] newRules;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			source = await sourcesRepository.GetByIdAsync(command.SourceId, ct)
				?? throw Errors.NotFoundSource(command.SourceId);

			var oldRules = await accessRulesRepository.GetSourceRulesAsync(source.Id);
			oldRulesId = oldRules.Select(x => x.Id).ToArray();
			await accessRulesRepository.RemoveRangeAsync(oldRules, ct);

			newRules = command.Rules.Select(x => new AccessRuleEntity(x.Type, sourceId: source.Id, userGuid: x.UserGuid, userGroupGuid: x.UserGroupGuid)).ToArray();
			await accessRulesRepository.AddRangeAsync(newRules, ct);

			var audit = new AuditEntity(command.User.Guid, "Изменены права доступа", sourceId: source.Id);
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
