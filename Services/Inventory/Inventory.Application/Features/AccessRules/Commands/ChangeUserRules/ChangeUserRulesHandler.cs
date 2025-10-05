using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Domain.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.AccessRules.Commands.ChangeUserRules;

public interface IChangeUserRulesHandler : ICommandHandler<ChangeUserRulesCommand, bool> { }

public class ChangeUserRulesHandler(
	IUsersRepository usersRepository,
	IAccessRulesRepository accessRulesRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache) : IChangeUserRulesHandler
{
	public async Task<bool> HandleAsync(ChangeUserRulesCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		User user;
		int[] oldRulesId;
		AccessRights[] newRules;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			user = await usersRepository.GetByIdAsync(command.UserGuid, ct)
				?? throw InventoryNotFoundException.NotFoundUser(command.UserGuid);

			var oldRules = await accessRulesRepository.GetUserRulesAsync(user.Guid);
			oldRulesId = oldRules.Select(x => x.Id).ToArray();
			await accessRulesRepository.RemoveRangeAsync(oldRules, ct);

			newRules = command.Rules.Select(x => new AccessRights(x.Type, userGuid: user.Guid, tagId: x.TagId, sourceId: x.SourceId, blockId: x.BlockId)).ToArray();
			await accessRulesRepository.AddRangeAsync(newRules, ct);

			var audit = new Log(command.User.Guid, "Изменены права доступа", userGuid: user.Guid);
			await auditRepository.AddAsync(audit, ct);
			await unitOfWork.SaveChangesAsync(ct);
		}
		catch
		{
			await unitOfWork.RollbackAsync(ct);
			throw;
		}

		await inventoryCache.UpdateAsync(state => state.WithAccessRules(oldRulesId, newRules));

		return true;
	}
}
