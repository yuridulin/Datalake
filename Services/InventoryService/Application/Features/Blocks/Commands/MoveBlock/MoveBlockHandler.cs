using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PrivateApi.Exceptions;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.MoveBlock;

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
				?? throw Errors.NotFoundBlock(command.BlockId);

			if (command.ParentId == command.BlockId)
				throw new ConflictException("Блок не может быть родителем самому себе");

			if (command.ParentId.HasValue && !await blocksRepository.ExistsAsync(command.ParentId.Value, ct))
				throw Errors.NotFoundBlock($"Родительский блок не найден по идентификатору: {command.ParentId}");

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
