using Datalake.Contracts.Models.Tags;
using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.Tags.Commands.CreateTag;

public interface ICreateTagHandler : ICommandHandler<CreateTagCommand, TagSimpleInfo> { }

public class CreateTagHandler(
	ITagsRepository tagsRepository,
	ISourcesRepository sourcesRepository,
	IBlocksRepository blocksRepository,
	IBlockTagsRepository blockTagsRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryStore inventoryCache,
	ILogger<CreateTagHandler> logger) :
		TransactionalCommandHandler<CreateTagCommand, TagSimpleInfo>(unitOfWork, logger, inventoryCache),
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

	public override async Task<TagSimpleInfo> ExecuteInTransactionAsync(CreateTagCommand command, CancellationToken ct = default)
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

		tag = Tag.Create(
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

		return new()
		{
			Guid = tag.Guid,
			Id = tag.Id,
			Name = tag.Name,
			Resolution = tag.Resolution,
			SourceId = tag.SourceId,
			SourceType = sourceType,
			Type = tag.Type,
			Description = tag.Description,
			AccessRule = new(0, AccessType.Viewer),
		};
	}

	public override IInventoryState UpdateCache(IInventoryState state) => blockTag == null
		? state.WithTag(tag)
		: state.WithTag(tag).WithTagBlocks(tag.Id, [blockTag]);
}
