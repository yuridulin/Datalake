using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.Sources.Commands.CreateSource;

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
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);
	}

	Source source = null!;

	public override async Task<int> ExecuteInTransactionAsync(CreateSourceCommand command, CancellationToken ct = default)
	{
		source = Source.CreateAsExternal(command.Type, address: command.Address, name: command.Name, description: command.Description);

		await sourcesRepository.AddAsync(source, ct);
		await _unitOfWork.SaveChangesAsync(ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Создан источник данных", sourceId: source.Id), ct);

		return source.Id;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithSource(source);
}
