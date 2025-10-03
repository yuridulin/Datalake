using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.Tags.Commands.DeleteTag;

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
		command.User.ThrowIfNoAccessToTag(AccessType.Manager, command.Id);
	}

	TagEntity tag = null!;

	public override async Task<bool> ExecuteInTransactionAsync(DeleteTagCommand command, CancellationToken ct = default)
	{
		tag = await tagsRepository.GetByIdAsync(command.Id, ct)
			?? throw InventoryNotFoundException.NotFoundTag(command.Id);

		tag.MarkAsDeleted();

		await tagsRepository.UpdateAsync(tag, ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Тег удален", tagId: tag.Id), ct);

		return true;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithTag(tag);
}
