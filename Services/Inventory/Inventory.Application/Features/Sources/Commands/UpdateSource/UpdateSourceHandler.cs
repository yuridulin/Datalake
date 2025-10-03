using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.Sources.Commands.UpdateSource;

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
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);
	}

	SourceEntity source = null!;

	public override async Task<int> ExecuteInTransactionAsync(UpdateSourceCommand command, CancellationToken ct = default)
	{
		source = await sourcesRepository.GetByIdAsync(command.SourceId, ct)
			?? throw InventoryNotFoundException.NotFoundSource(command.SourceId);

		source.UpdateType(command.Type);
		source.UpdateProperties(name: command.Name, description: command.Description, address: command.Address);

		await sourcesRepository.UpdateAsync(source, ct);
		await _unitOfWork.SaveChangesAsync(ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Источник данных удален", sourceId: source.Id), ct);

		return source.Id;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithSource(source);
}