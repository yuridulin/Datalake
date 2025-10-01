using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Blocks.Commands.UpdateBlock;

public interface IUpdateBlockHandler : ICommandHandler<UpdateBlockCommand, int> { }

public class UpdateBlockHandler(
	IBlocksRepository blocksRepository,
	IBlockTagsRepository blockTagsRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache,
	ILogger<UpdateBlockHandler> logger) :
		TransactionalCommandHandler<UpdateBlockCommand, int>(unitOfWork, logger, inventoryCache),
		IUpdateBlockHandler
{
	private BlockEntity block = null!;
	private BlockTagEntity[] blockTags = null!;

	public override void CheckPermissions(UpdateBlockCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.NoAccess);
	}

	public override async Task<int> ExecuteInTransactionAsync(UpdateBlockCommand command, CancellationToken ct = default)
	{
		block = await blocksRepository.GetByIdAsync(command.BlockId, ct)
			?? throw Errors.NotFoundBlock(command.BlockId);

		block.UpdateName(command.Name);
		block.UpdateDescription(command.Description);

		var existBlockTags = await blockTagsRepository.GetByBlockIdAsync(block.Id);
		await blockTagsRepository.RemoveRangeAsync(existBlockTags);

		blockTags = command.Tags.Select(x => new BlockTagEntity(block.Id, x.TagId, x.LocalName, x.Relation)).ToArray();
		await blockTagsRepository.AddRangeAsync(blockTags);

		var audit = new AuditEntity(command.User.Guid, $"Изменения: diff", blockId: block.Id);
		await auditRepository.AddAsync(audit, ct);

		return block.Id;
	}

	public override InventoryState UpdateCache(InventoryState state) => state.WithBlock(block).WithBlockTags(block.Id, blockTags);
}