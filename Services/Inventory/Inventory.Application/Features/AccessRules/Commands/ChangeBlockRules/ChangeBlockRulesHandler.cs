using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Models;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeBlockRules;

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

		Block block;
		int[] oldRulesId;
		AccessRule[] newRules;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			block = await blocksRepository.GetByIdAsync(command.BlockId, ct)
				?? throw InventoryNotFoundException.NotFoundBlock(command.BlockId);

			var oldRules = await accessRulesRepository.GetBlockRulesAsync(block.Id);
			oldRulesId = oldRules.Select(x => x.Id).ToArray();
			await accessRulesRepository.RemoveRangeAsync(oldRules, ct);

			newRules = command.Rules.Select(x => new AccessRule(x.Type, blockId: block.Id, userGuid: x.UserGuid, userGroupGuid: x.UserGroupGuid)).ToArray();
			await accessRulesRepository.AddRangeAsync(newRules, ct);

			var audit = new Log(command.User.Guid, "Изменены права доступа", blockId: block.Id);
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
