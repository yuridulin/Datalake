using Datalake.Gateway.Application.Features.UsersAccess.Commands.UpdateUsersAccess;
using Datalake.Shared.Application.Models;
using MassTransit;

namespace Datalake.Gateway.Host.Consumers;

/// <summary>
/// Обработка события пересчета прав доступа
/// </summary>
public class AccessUpdateConsumer(
	IUpdateUsersAccessHandler handler,
	ILogger<AccessUpdateConsumer> logger) : IConsumer<AccessUpdateMessage>
{
	/// <inheritdoc/>
	public Task Consume(ConsumeContext<AccessUpdateMessage> context)
	{
		var message = context.Message;

		if (logger.IsEnabled(LogLevel.Information))
			logger.LogInformation("Получено событие обновления прав доступа версии {version}", message.Version);

		_ = handler.HandleAsync(new() { Guids = message.AffectedUsers });

		return Task.CompletedTask;
	}
}
