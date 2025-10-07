using Datalake.Contracts.Internal.Messages;
using Datalake.Data.Application.Features.UsersAccess.Commands.UpdateUsersAccess;
using MassTransit;

namespace Datalake.Data.Host.Consumers;

public class AccessUpdateConsumer(
	IUpdateUsersAccessHandler handler,
	ILogger<AccessUpdateConsumer> logger) : IConsumer<AccessUpdateMessage>
{
	public Task Consume(ConsumeContext<AccessUpdateMessage> context)
	{
		var message = context.Message;

		logger.LogInformation("Notification processed for event: {timestamp}", message.Timestamp);

		_ = handler.HandleAsync(new() { });

		return Task.CompletedTask;
	}
}
