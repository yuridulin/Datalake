using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.CreateBlock;

public interface ICreateBlockHandler : ICommandHandler<CreateBlockCommand, int> { }

public class CreateBlockHandler(
	IUnitOfWork unitOfWork,
	IBlocksRepository blocksRepository,
	IAuditRepository auditRepository,
	IInventoryCache inventoryCache) : ICreateBlockHandler
{
	public async Task<int> HandleAsync(CreateBlockCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Manager);

		Block block = string.IsNullOrEmpty(command.Name)
			? new(command.ParentId)
			: new(command.ParentId, command.Name, command.Description);

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			await blocksRepository.AddAsync(block, ct);
			await unitOfWork.SaveChangesAsync(ct);

			if (string.IsNullOrWhiteSpace(block.Name))
			{
				block.UpdateName($"Блок {block.Id}");
				await blocksRepository.UpdateAsync(block, ct);
				await unitOfWork.SaveChangesAsync(ct);
			}

			var audit = new Log(command.User.Guid, $"Создан новый блок: {block.Name}", blockId: block.Id);
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
