using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Interfaces.Persistent;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;

namespace Datalake.InventoryService.Application.Features.Tags.Commands.DeleteTag;

public interface IDeleteTagHandler : ICommandHandler<DeleteTagCommand, bool> { }

public class DeleteTagHandler(
	ITagsRepository tagsRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache,
	ILogger<DeleteTagHandler> logger) :
		TransactionalCommandHandler<DeleteTagCommand, bool>(unitOfWork, logger, inventoryCache),
		IDeleteTagHandler
{
	public override void CheckPermissions(DeleteTagCommand command)
	{
		command.User.ThrowIfNoAccessToTag(PublicApi.Enums.AccessType.Manager, command.Id);
	}

	TagEntity tag = null!;

	public override async Task<bool> ExecuteInTransactionAsync(DeleteTagCommand command, CancellationToken ct = default)
	{
		tag = await tagsRepository.GetByIdAsync(command.Id, ct)
			?? throw Errors.NotFoundTag(command.Id);

		tag.MarkAsDeleted();

		await tagsRepository.UpdateAsync(tag, ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Тег удален", tagId: tag.Id), ct);

		return true;
	}

	public override InventoryState UpdateCache(InventoryState state) => state.WithTag(tag);
}
