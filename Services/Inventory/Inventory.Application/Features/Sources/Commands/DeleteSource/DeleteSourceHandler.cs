using Datalake.Contracts.Public.Enums;
using Datalake.Domain.Entities;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.Sources.Commands.DeleteSource;

public interface IDeleteSourceHandler : ICommandHandler<DeleteSourceCommand, int> { }

public class DeleteSourceHandler(
	ISourcesRepository sourcesRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache,
	ILogger<DeleteSourceHandler> logger) :
		TransactionalCommandHandler<DeleteSourceCommand, int>(unitOfWork, logger, inventoryCache),
		IDeleteSourceHandler
{
	public override void CheckPermissions(DeleteSourceCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);
	}

	Source source = null!;

	public override async Task<int> ExecuteInTransactionAsync(DeleteSourceCommand command, CancellationToken ct = default)
	{
		source = await sourcesRepository.GetByIdAsync(command.SourceId, ct)
			?? throw InventoryNotFoundException.NotFoundSource(command.SourceId);

		source.MarkAsDeleted();

		await sourcesRepository.UpdateAsync(source, ct);
		await _unitOfWork.SaveChangesAsync(ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Источник данных удален", sourceId: source.Id), ct);

		return source.Id;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithSource(source);
}