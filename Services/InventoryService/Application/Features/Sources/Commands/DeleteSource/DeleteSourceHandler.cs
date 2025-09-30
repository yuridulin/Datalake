using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Features.Sources.Commands.DeleteSource;

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
		command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Admin);
	}

	SourceEntity source = null!;

	public override async Task<int> ExecuteInTransactionAsync(DeleteSourceCommand command, CancellationToken ct = default)
	{
		source = await sourcesRepository.GetByIdAsync(command.SourceId, ct)
			?? throw Errors.NotFoundSource(command.SourceId);

		source.MarkAsDeleted();

		await sourcesRepository.UpdateAsync(source, ct);
		await _unitOfWork.SaveChangesAsync(ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Источник данных удален", sourceId: source.Id), ct);

		return source.Id;
	}

	public override InventoryState UpdateCache(InventoryState state) => state.WithSource(source);
}