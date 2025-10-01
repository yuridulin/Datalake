using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Features.Sources.Commands.CreateSource;

public interface ICreateSourceHandler : ICommandHandler<CreateSourceCommand, int> { }

public class CreateSourceHandler(
	ISourcesRepository sourcesRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache,
	ILogger<CreateSourceHandler> logger) :
		TransactionalCommandHandler<CreateSourceCommand, int>(unitOfWork, logger, inventoryCache),
		ICreateSourceHandler
{
	public override void CheckPermissions(CreateSourceCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Admin);
	}

	SourceEntity source = null!;

	public override async Task<int> ExecuteInTransactionAsync(CreateSourceCommand command, CancellationToken ct = default)
	{
		source =  new(command.Type, address: command.Address, name: command.Name, description: command.Description);

		await sourcesRepository.AddAsync(source, ct);
		await _unitOfWork.SaveChangesAsync(ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Создан источник данных", sourceId: source.Id), ct);

		return source.Id;
	}

	public override InventoryState UpdateCache(InventoryState state) => state.WithSource(source);
}
