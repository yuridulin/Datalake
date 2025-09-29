using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeBlockRules;

public interface IChangeBlockRulesHandler : ICommandHandler<ChangeBlockRulesCommand, bool> { }

public class ChangeBlockRulesHandler(
	IBlocksRepository blocksRepository,
	IAccessRulesRepository accessRulesRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache) : IChangeBlockRulesHandler
{
	public async Task<bool> HandleAsync(ChangeBlockRulesCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		BlockEntity block;
		int[] oldRulesId;
		AccessRuleEntity[] newRules;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			block = await blocksRepository.GetByIdAsync(command.BlockId, ct)
				?? throw Errors.NotFoundBlock(command.BlockId);

			var oldRules = await accessRulesRepository.GetBlockRulesAsync(block.Id);
			oldRulesId = oldRules.Select(x => x.Id).ToArray();
			await accessRulesRepository.RemoveRangeAsync(oldRules, ct);

			newRules = command.Rules.Select(x => new AccessRuleEntity(x.Type, blockId: block.Id, userGuid: x.UserGuid, userGroupGuid: x.UserGroupGuid)).ToArray();
			await accessRulesRepository.AddRangeAsync(newRules, ct);

			var audit = new AuditEntity(command.User.Guid, "Изменены права доступа", blockId: block.Id);
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
