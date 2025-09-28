using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.UpdateBlock;

public class UpdateBlockHandler(
	IUnitOfWork unitOfWork,
	IBlocksRepository blocksRepository,
	IAuditRepository auditRepository,
	IInventoryCache inventoryCache) : IUpdateBlockHandler
{
	public async Task<int> HandleAsync(UpdateBlockCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.NoAccess);

		BlockEntity block;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			block = await blocksRepository.GetByIdAsync(command.BlockId, ct)
				?? throw Errors.NotFoundBlock(command.BlockId);

			block.UpdateName(command.Name);
			block.UpdateDescription(command.Description);

			var audit = new AuditEntity(command.User.Guid, $"Изменения: diff", blockId: block.Id);
			await auditRepository.AddAsync(audit, ct);
			await unitOfWork.SaveChangesAsync(ct);

			await unitOfWork.CommitAsync(ct);
		}
		catch
		{
			await unitOfWork.RollbackAsync(ct);
			throw;
		}

		await inventoryCache.UpdateAsync(state => state with
		{
			Blocks = state.Blocks.RemoveAll(x => x.Id == block.Id).Add(block)
		});

		return block.Id;
	}
}
