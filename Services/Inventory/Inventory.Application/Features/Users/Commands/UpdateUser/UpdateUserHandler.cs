using Datalake.Domain.Entities;
using Datalake.Domain.Enums;
using Datalake.Inventory.Application.Abstractions;
using Datalake.Inventory.Application.Exceptions;
using Datalake.Inventory.Application.Interfaces.InMemory;
using Datalake.Inventory.Application.Interfaces.Persistent;
using Datalake.Inventory.Application.Repositories;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Inventory.Application.Features.Users.Commands.UpdateUser;

public interface IUpdateUserHandler : ICommandHandler<UpdateUserCommand, bool> { }

public class UpdateUserHandler(
	IUsersRepository usersRepository,
	IAccessRulesRepository accessRulesRepository,
	IAuditRepository auditRepository,
	IUnitOfWork unitOfWork,
	ILogger<UpdateUserHandler> logger,
	IInventoryCache inventoryCache) :
		TransactionalCommandHandler<UpdateUserCommand, bool>(unitOfWork, logger, inventoryCache),
		IUpdateUserHandler
{
	public override void CheckPermissions(UpdateUserCommand command)
	{
		command.User.ThrowIfNoGlobalAccess(AccessType.Admin);
	}

	User user = null!;

	public override async Task<bool> ExecuteInTransactionAsync(UpdateUserCommand command, CancellationToken ct = default)
	{
		user = await usersRepository.GetByIdAsync(command.Guid, ct)
			?? throw InventoryNotFoundException.NotFoundUser(command.Guid);

		if (user.Type == UserType.Local && !string.IsNullOrEmpty(command.Login))
		{
			var userWithNewLogin = await usersRepository.GetByLoginAsync(command.Login, ct);
			if (userWithNewLogin != null && userWithNewLogin.Guid != user.Guid)
			{
				throw new ApplicationException("Указанный логин уже используется другим пользователем");
			}
		}

		user.Update(
			login: command.Login,
			passwordString: command.Password,
			email: command.Email,
			fullName: command.FullName);

		await usersRepository.UpdateAsync(user, ct);
		await auditRepository.AddAsync(new(command.User.Guid, "Учетная запись изменена: diff", userGuid: user.Guid), ct);

		var globalRule = await accessRulesRepository.GetUserGlobalRuleAsync(user.Guid, ct);
		if (globalRule != null && globalRule.AccessType != command.AccessType)
		{
			globalRule.UpdateAccess(command.AccessType);
			await accessRulesRepository.UpdateAsync(globalRule, ct);
			await auditRepository.AddAsync(new(command.User.Guid, "Изменен уровень общего доступа: diff", userGuid: user.Guid), ct);
		}

		return true;
	}

	public override IInventoryCacheState UpdateCache(IInventoryCacheState state) => state.WithUser(user);
}
