using Datalake.Data.Application.Features.UserAccess.Commands.Update;
using Datalake.Shared.Application.Attributes;
using Datalake.Shared.Application.Models;
using MassTransit;

namespace Datalake.Data.Host.Consumers;

/// <summary>
/// Обработка события обновления рассчитанных прав доступа
/// </summary>
[Scoped]
public class AccessUpdateConsumer(
	IUpdateUsersAccessHandler handler,
	ILogger<AccessUpdateConsumer> logger) : IConsumer<AccessUpdateMessage>
{
	/// <summary>
	/// Обработка события обновления рассчитанных прав доступа
	/// </summary>
	public async Task Consume(ConsumeContext<AccessUpdateMessage> context)
	{
		var message = context.Message;

		if (logger.IsEnabled(LogLevel.Information))
			logger.LogInformation("Получено событие обновления прав доступа версии {version}", message.Version);

		await handler.HandleAsync(new() { Guids = message.AffectedUsers, IsAllUsers = false, });
	}
}
