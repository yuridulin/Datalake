using Datalake.InventoryService.Application.Abstractions;
using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.InventoryService.Application.Interfaces.InMemory;
using Datalake.InventoryService.Application.Repositories;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;

namespace Datalake.InventoryService.Application.Features.Users.Commands.UpdateUser;

public interface IUpdateUserHandler : ICommandHandler<UpdateUserCommand, bool> { }

public class UpdateUserHandler(
	IUsersRepository usersRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	ILogger<UpdateUserHandler> logger,
	IInventoryCache inventoryCache) :
		TransactionalCommandHandler<UpdateUserCommand, bool>(unitOfWork, logger, inventoryCache),
		IUpdateUserHandler
{
	public override void CheckPermissions(UpdateUserCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(PublicApi.Enums.AccessType.Admin);
	}

	UserEntity user = null!;

	public override async Task<bool> ExecuteInTransactionAsync(UpdateUserCommand command, CancellationToken ct = default)
	{
		user = await usersRepository.GetByIdAsync(command.Guid, ct)
			?? throw Errors.NotFoundUser(command.Guid);

		user.Update(
			type: command.Type,
			fullName: command.FullName,
			login: command.Login,
			passwordString: command.Password,
			energoIdGuid: command.EnergoIdGuid,
			host: command.StaticHost,
			generateNewHash: command.GenerateNewHash);

		await usersRepository.UpdateAsync(user, ct);

		await auditRepository.AddAsync(new(command.User.Guid, "Учетная запись изменена: diff", userGuid: user.Guid), ct);

		return true;
	}

	public override InventoryState UpdateCache(InventoryState state) => state.WithUser(user);
}
