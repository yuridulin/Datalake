using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Microsoft.Extensions.Logging;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Commands.CreateTag;

public interface ICreateTagHandler : ICommandHandler<CreateTagCommand, int> { }

public class CreateTagHandler(
	ITagsRepository tagsRepository,
	ISourcesRepository sourcesRepository,
	IBlocksRepository blocksRepository,
	IBlockTagsRepository blockTagsRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache,
	ILogger<CreateTagHandler> logger) :
		TransactionalCommandHandler<CreateTagCommand, int>(unitOfWork, logger, inventoryCache),
		ICreateTagHandler
{
	public override void CheckPermissions(CreateTagCommand command)
	{
		if (command.BlockId.HasValue)
		{
			command.User.ThrowIfNoAccessToBlock(AccessType.Manager, command.BlockId.Value);
		}
		else if (command.SourceId.HasValue)
		{
			command.User.ThrowIfNoAccessToSource(AccessType.Viewer, command.SourceId.Value);
		}
		else
		{
			command.User.ThrowIfNoGlobalAccess(AccessType.Manager);
		}
	}

	Tag tag = null!;
	BlockTag? blockTag = null;

	public override async Task<int> ExecuteInTransactionAsync(CreateTagCommand command, CancellationToken ct = default)
	{
		Source? source = null;
		SourceType sourceType = SourceType.Manual;

		if (command.SourceId.HasValue)
		{
			source = await sourcesRepository.GetByIdAsync(command.SourceId.Value, ct)
				?? throw InventoryNotFoundException.NotFoundSource(command.SourceId.Value);

			sourceType = source.Type;
		}

		if (command.BlockId.HasValue && !await blocksRepository.ExistsAsync(command.BlockId.Value, ct))
		{
			throw InventoryNotFoundException.NotFoundBlock(command.BlockId.Value);
		}

		tag = new(
			type: command.Type,
			sourceType: sourceType,
			sourceId: command.SourceId,
			sourceItem: command.SourceItem);

		await tagsRepository.AddAsync(tag, ct);
		await _unitOfWork.SaveChangesAsync(ct);

		tag.SetGenericName(sourceType, source?.Name);

		if (command.BlockId.HasValue)
		{
			blockTag = new(command.BlockId.Value, tag.Id, tag.Name);
			await blockTagsRepository.AddAsync(blockTag, ct);
		}

		await auditRepository.AddAsync(new(command.User.Guid, "Создан тег: " + tag.Type, tagId: tag.Id), ct);

		return tag.Id;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => blockTag == null
		? state.WithTag(tag)
		: state.WithTag(tag).WithTagBlocks(tag.Id, [blockTag]);
}
