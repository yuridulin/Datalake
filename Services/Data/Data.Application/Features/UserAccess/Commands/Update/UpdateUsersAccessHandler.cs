using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Interfaces;
using Datalake.Shared.Application.Interfaces.AccessRules;
using Microsoft.Extensions.Logging;

namespace Datalake.Data.Application.Features.UserAccess.Commands.Update;

public interface IUpdateUsersAccessHandler : ICommandHandler<UpdateUsersAccessCommand, bool> { }

[Scoped]
public class UpdateUsersAccessHandler(
	IUserAccessValuesRepository userAccessRepository,
	IUsersAccessStore userAccessCache,
	ILogger<UpdateUsersAccessHandler> logger) : IUpdateUsersAccessHandler
{
	public async Task<bool> HandleAsync(UpdateUsersAccessCommand command, CancellationToken ct = default)
	{
		if (logger.IsEnabled(LogLevel.Information))
		{
			if (command.IsAllUsers)
				logger.LogInformation("Получено событие обновления рассчитанных прав доступа. Затронуты все ользователи");
			else
				logger.LogInformation("Получено событие обновления рассчитанных прав доступа. Пользователей затронуто: {count}", command.Guids.Count());
		}

		try
		{
			var updatedAccess = command.IsAllUsers
				? await userAccessRepository.GetAllAsync(ct)
				: await userAccessRepository.GetMultipleAsync(command.Guids, ct);

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
