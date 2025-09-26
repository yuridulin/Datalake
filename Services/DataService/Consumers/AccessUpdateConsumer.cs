using Datalake.PrivateApi.Messages;
using MassTransit;

namespace Datalake.DataService.Consumers;

public class AccessUpdateConsumer(
	ILogger<AccessUpdateConsumer> logger) : IConsumer<AccessUpdateMessage>
{
	public async Task Consume(ConsumeContext<AccessUpdateMessage> context)
	{
		var message = context.Message;

		// Здесь ваша логика обработки
		await Task.Delay(1000); // Имитация обработки

		logger.LogInformation("Notification processed for event: {timestamp}", message.Timestamp);
	}
}
