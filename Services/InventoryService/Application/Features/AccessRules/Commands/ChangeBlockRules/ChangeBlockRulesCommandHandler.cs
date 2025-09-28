
using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeBlockRules;

public class ChangeBlockRulesCommandHandler(
	IBlocksRepository blocksRepository,
	IAccessRulesRepository accessRulesRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache) : IChangeBlockRulesCommandHandler
{
	public async Task<bool> HandleAsync(ChangeBlockRulesCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		BlockEntity block;
		BlockTagEntity[] blockTags;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			block = await blocksRepository.GetByIdAsync(command.BlockId, ct)
				?? throw Errors.NotFoundBlock(command.BlockId);

			var existRules = await accessRulesRepository.GetBlockRulesAsync(block.Id);
			await accessRulesRepository.RemoveRangeAsync(existRules, ct);

			rules = command.Tags.Select(x => new BlockTagEntity(block.Id, x.TagId, x.LocalName, x.Relation)).ToArray();
			await accessRulesRepository.AddRangeAsync(rules);

			var audit = new AuditEntity(command.User.Guid, $"Изменения: diff", blockId: block.Id);
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
			Blocks = state.Blocks.Add(block)
		});

		return true;
	}
}
