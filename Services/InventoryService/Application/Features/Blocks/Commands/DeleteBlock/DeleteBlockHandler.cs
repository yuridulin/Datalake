using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.DeleteBlock;

public interface IDeleteBlockHandler : ICommandHandler<DeleteBlockCommand, bool> { }

public class DeleteBlockHandler(
	IUnitOfWork unitOfWork,
	IBlocksRepository blocksRepository,
	IAuditRepository auditRepository,
	IInventoryCache inventoryCache) : IDeleteBlockHandler
{
	public async Task<bool> HandleAsync(DeleteBlockCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoAccessToBlock(AccessType.Manager, command.BlockId);

		BlockEntity block;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			block = await blocksRepository.GetByIdAsync(command.BlockId, ct)
				?? throw Errors.NotFoundBlock(command.BlockId);

			block.MarkAsDeleted();
			await blocksRepository.UpdateAsync(block, ct);

			var audit = new AuditEntity(command.User.Guid, $"Блок удален", blockId: block.Id);
			await auditRepository.AddAsync(audit, ct);

			await unitOfWork.SaveChangesAsync(ct);
			await unitOfWork.CommitAsync(ct);
		}
		catch
		{
			await unitOfWork.RollbackAsync(ct);
			throw;
		}

		await inventoryCache.UpdateAsync(state => state.WithBlock(block));

		return true;
	}
}
