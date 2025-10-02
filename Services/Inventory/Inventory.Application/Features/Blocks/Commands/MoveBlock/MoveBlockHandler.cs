using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Inventory.Domain.Entities;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.MoveBlock;

public interface IMoveBlockHandler : ICommandHandler<MoveBlockCommand, int> { }

public class MoveBlockHandler(
	IUnitOfWork unitOfWork,
	IBlocksRepository blocksRepository,
	IAuditRepository auditRepository,
	IInventoryCache inventoryCache) : IMoveBlockHandler
{
	public async Task<int> HandleAsync(MoveBlockCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Manager);

		BlockEntity block;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			block = await blocksRepository.GetByIdAsync(command.BlockId, ct)
				?? throw InventoryNotFoundException.NotFoundBlock(command.BlockId);

			if (command.ParentId.HasValue && !await blocksRepository.ExistsAsync(command.ParentId.Value, ct))
				throw InventoryNotFoundException.NotFoundBlock($"Родительский блок не найден по идентификатору: {command.ParentId}");

			block.UpdateParent(command.ParentId);

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

		await inventoryCache.UpdateAsync(state => state.WithBlock(block));

		return block.Id;
	}
}
