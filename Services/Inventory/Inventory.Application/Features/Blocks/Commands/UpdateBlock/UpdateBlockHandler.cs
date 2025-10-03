using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.Blocks.Commands.UpdateBlock;

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
		command.User.ThrowIfNoAccessToBlock(AccessType.Manager, command.BlockId);
	}

	public override async Task<int> ExecuteInTransactionAsync(UpdateBlockCommand command, CancellationToken ct = default)
	{
		block = await blocksRepository.GetByIdAsync(command.BlockId, ct)
			?? throw InventoryNotFoundException.NotFoundBlock(command.BlockId);

		block.UpdateName(command.Name);
		block.UpdateDescription(command.Description);

		var existBlockTags = await blockTagsRepository.GetByBlockIdAsync(block.Id, ct);
		await blockTagsRepository.RemoveRangeAsync(existBlockTags, ct);

		if (command.Tags.Any())
		{
			blockTags = command.Tags.Select(x => new BlockTagEntity(block.Id, x.TagId, x.LocalName, x.Relation)).ToArray();
			await blockTagsRepository.AddRangeAsync(blockTags, ct);
		}

		var audit = new AuditEntity(command.User.Guid, $"Изменения: diff", blockId: block.Id);
		await auditRepository.AddAsync(audit, ct);

		return block.Id;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithBlock(block).WithBlockTags(block.Id, blockTags);
}