using Datalake.Shared.Application.Interfaces;
using Datalake.Shared.Application.Interfaces.AccessRules;
using Microsoft.Extensions.Logging;

namespace Datalake.Gateway.Application.Features.UserAccess.Commands.Update;

public interface IUpdateUsersAccessHandler : ICommandHandler<UpdateUsersAccessCommand, bool> { }

public class UpdateUsersAccessHandler(
	IUserAccessRepository userAccessRepository,
	IUsersAccessStore userAccessCache,
	ILogger<UpdateUsersAccessHandler> logger) : IUpdateUsersAccessHandler
{
	public async Task<bool> HandleAsync(UpdateUsersAccessCommand command, CancellationToken ct = default)
	{
		if (logger.IsEnabled(LogLevel.Information))
			logger.LogInformation("Получено событие обновления рассчитанных прав доступа. Пользователей затронуто: {count}", command.Guids.Count());

		try
		{
			var updatedAccess = command.Guids.Any()
				? await userAccessRepository.GetMultipleAsync(command.Guids, ct)
				: await userAccessRepository.GetAllAsync(ct);

			userAccessCache.Set(updatedAccess);
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
