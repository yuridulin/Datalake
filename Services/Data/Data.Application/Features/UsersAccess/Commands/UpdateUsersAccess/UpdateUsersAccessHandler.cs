using Datalake.Data.Application.Interfaces;
using Datalake.Data.Application.Interfaces.Cache;
using Datalake.Shared.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Application.Features.UsersAccess.Commands.UpdateUsersAccess;

public interface IUpdateUsersAccessHandler : ICommandHandler<UpdateUsersAccessCommand, bool> { }

public class UpdateUsersAccessHandler(
	IInventoryApiClient inventoryApiClient,
	IUserAccessStore userAccessStore,
	ILogger<UpdateUsersAccessHandler> logger) : IUpdateUsersAccessHandler
{
	public async Task<bool> HandleAsync(UpdateUsersAccessCommand command, CancellationToken ct = default)
	{
		if (logger.IsEnabled(LogLevel.Information))
			logger.LogInformation("Получено событие обновления рассчитанных прав доступа. Пользователей затронуто: {count}", command.Guids.Count());

		try
		{
			var updatedAccess = await inventoryApiClient.GetCalculatedAccessAsync(command.Guids, ct); // старый вызов REST
			await userAccessStore.UpdateAsync(updatedAccess);

			logger.LogInformation("Рассчитанные права доступа получены и записаны в кэш");
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Ошибка при обновлении рассчитанных прав доступа");
			throw;
		}

		return true;
	}
}
