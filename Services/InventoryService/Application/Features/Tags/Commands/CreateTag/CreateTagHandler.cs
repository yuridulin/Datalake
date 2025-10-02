using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;

namespace Datalake.InventoryService.Application.Features.Tags.Commands.CreateTag;

public interface ICreateTagHandler : ICommandHandler<CreateTagCommand, int> { }

public class CreateTagHandler(
	ITagsRepository tagsRepository,
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
			command.User.ThrowIfNoAccessToBlock(PublicApi.Enums.AccessType.Manager, command.BlockId.Value);
		}
		else if (command.SourceId.HasValue)
		{
			command.User.ThrowIfNoAccessToSource(PublicApi.Enums.AccessType.Viewer, command.SourceId.Value);
		}
		else
		{
			command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Manager);
		}
	}

	TagEntity tag = null!;
	BlockTagEntity? blockTag = null;

	public override async Task<int> ExecuteInTransactionAsync(CreateTagCommand command, CancellationToken ct = default)
	{
		tag = new(
			type: command.Type,
			sourceId: command.SourceId,
			sourceItem: command.SourceItem);

		await tagsRepository.AddAsync(tag, ct);
		await _unitOfWork.SaveChangesAsync(ct);

		tag.SetGenericName();

		if (command.BlockId.HasValue)
		{
			blockTag = new(command.BlockId.Value, tag.Id, tag.Name);
			await blockTagsRepository.AddAsync(blockTag, ct);
		}

		await auditRepository.AddAsync(new(command.User.Guid, "Создан тег: " + tag.Type, tagId: tag.Id), ct);

		return tag.Id;
	}

	public override InventoryState UpdateCache(InventoryState state) => blockTag == null
		? state.WithTag(tag)
		: state.WithTag(tag).WithTagBlocks(tag.Id, [blockTag]);
}
