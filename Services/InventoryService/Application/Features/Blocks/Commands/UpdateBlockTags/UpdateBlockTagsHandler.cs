using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.UpdateBlockTags;

public class UpdateBlockTagsHandler(
	IUnitOfWork unitOfWork,
	IBlocksRepository blocksRepository,
	IBlockTagsRepository blockTagsRepository,
	IAuditRepository auditRepository,
	IInventoryCache inventoryCache) : IUpdateBlockTagsHandler
{
	public async Task<int> HandleAsync(UpdateBlockTagsCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.NoAccess);

		BlockEntity block;
		BlockTagEntity[] blockTags;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			block = await blocksRepository.GetByIdAsync(command.BlockId, ct)
				?? throw Errors.NotFoundBlock(command.BlockId);

			var existBlockTags = await blockTagsRepository.GetByBlockIdAsync(block.Id);
			await blockTagsRepository.RemoveRangeAsync(existBlockTags);

			blockTags = command.Tags.Select(x => new BlockTagEntity(block.Id, x.TagId, x.LocalName, x.Relation)).ToArray();
			await blockTagsRepository.AddRangeAsync(blockTags);

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
			BlockTags = state.BlockTags.RemoveAll(x => x.BlockId == block.Id).AddRange(blockTags)
		});

		return block.Id;
	}
}
