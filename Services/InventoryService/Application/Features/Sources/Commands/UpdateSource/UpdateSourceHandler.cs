using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Features.Sources.Commands.UpdateSource;

public interface IUpdateSourceHandler : ICommandHandler<UpdateSourceCommand, int> { }

public class UpdateSourceHandler(
	ISourcesRepository sourcesRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache,
	ILogger<UpdateSourceHandler> logger) :
		TransactionalCommandHandler<UpdateSourceCommand, int>(unitOfWork, logger, inventoryCache),
		IUpdateSourceHandler
{
	public override void CheckPermissions(UpdateSourceCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Admin);
	}

	SourceEntity source = null!;

	public override async Task<int> ExecuteInTransactionAsync(UpdateSourceCommand command, CancellationToken ct = default)
	{
		source = await sourcesRepository.GetByIdAsync(command.SourceId, ct)
			?? throw Errors.NotFoundSource(command.SourceId);

		source.UpdateType(command.Type);
		source.UpdateProperties(name: command.Name, description: command.Description, address: command.Address);

		await sourcesRepository.UpdateAsync(source, ct);
		await _unitOfWork.SaveChangesAsync(ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Источник данных удален", sourceId: source.Id), ct);

		return source.Id;
	}

	public override InventoryState UpdateCache(InventoryState state) => state.WithSource(source);
}