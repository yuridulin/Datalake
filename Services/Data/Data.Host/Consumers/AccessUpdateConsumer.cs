using Datalake.Data.Application.Features.UsersAccess.Commands.UpdateUsersAccess;
using Datalake.Shared.Application.Models;
using MassTransit;

namespace Datalake.Data.Host.Consumers;

public class AccessUpdateConsumer(
	IUpdateUsersAccessHandler handler,
	ILogger<AccessUpdateConsumer> logger) : IConsumer<AccessUpdateMessage>
{
	public Task Consume(ConsumeContext<AccessUpdateMessage> context)
	{
		var message = context.Message;

		logger.LogInformation("Получено событие обновления прав доступа версии {version}", message.Version);

		_ = handler.HandleAsync(new() { Guids = message.AffectedUsers });

		return Task.CompletedTask;
	}
}
