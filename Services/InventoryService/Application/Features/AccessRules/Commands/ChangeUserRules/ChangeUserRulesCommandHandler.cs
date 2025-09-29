using Datalake.InventoryService.Application.Constants;
using Datalake.InventoryService.Domain.Entities;
using Datalake.InventoryService.Domain.Repositories;
using Datalake.InventoryService.Infrastructure.Cache.Inventory;
using Datalake.InventoryService.Infrastructure.Database.Abstractions;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.AccessRules.Commands.ChangeUserRules;

public class ChangeUserRulesCommandHandler(
	IUsersRepository usersRepository,
	IAccessRulesRepository accessRulesRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	IInventoryCache inventoryCache) : IChangeUserRulesCommandHandler
{
	public async Task<bool> HandleAsync(ChangeUserRulesCommand command, CancellationToken ct = default)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);

		UserEntity user;
		int[] oldRulesId;
		AccessRuleEntity[] newRules;

		await unitOfWork.BeginTransactionAsync(ct);

		try
		{
			user = await usersRepository.GetByIdAsync(command.UserGuid, ct)
				?? throw Errors.NotFoundUser(command.UserGuid);

			var oldRules = await accessRulesRepository.GetUserRulesAsync(user.Guid);
			oldRulesId = oldRules.Select(x => x.Id).ToArray();
			await accessRulesRepository.RemoveRangeAsync(oldRules, ct);

			newRules = command.Rules.Select(x => new AccessRuleEntity(x.Type, userGuid: user.Guid, tagId: x.TagId, sourceId: x.SourceId, blockId: x.BlockId)).ToArray();
			await accessRulesRepository.AddRangeAsync(newRules, ct);

			var audit = new AuditEntity(command.User.Guid, "Изменены права доступа", userGuid: user.Guid);
			await auditRepository.AddAsync(audit, ct);
			await unitOfWork.SaveChangesAsync(ct);
		}
		catch
		{
			await unitOfWork.RollbackAsync(ct);
			throw;
		}

		await inventoryCache.UpdateAsync(state => state with
		{
			AccessRules = state.AccessRules
				.RemoveAll(x => oldRulesId.Contains(x.Id))
				.AddRange(newRules)
		});

		return true;
	}
}
