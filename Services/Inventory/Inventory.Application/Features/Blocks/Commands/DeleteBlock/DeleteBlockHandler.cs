using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.DeleteBlock;

public interface IDeleteBlockHandler : ICommandHandler<DeleteBlockCommand, bool> { }

public class DeleteBlockHandler(
	IUnitOfWork unitOfWork,
	IBlocksRepository blocksRepository,
	IAuditRepository auditRepository,
	ICalculatedAccessRulesRepository calculatedAccessRulesRepository,
	IInventoryCache inventoryCache) : IDeleteBlockHandler
{
	public async Task<bool> HandleAsync(DeleteBlockCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoAccessToBlock(AccessType.Manager, command.BlockId);

		Block block;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			block = await blocksRepository.GetByIdAsync(command.BlockId, ct)
				?? throw InventoryNotFoundException.NotFoundBlock(command.BlockId);

			block.MarkAsDeleted();
			await blocksRepository.UpdateAsync(block, ct);

			await calculatedAccessRulesRepository.RemoveByBlockId(command.BlockId, ct);
			var audit = new AuditLog(command.User.Guid, $"Блок удален", blockId: block.Id);
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
