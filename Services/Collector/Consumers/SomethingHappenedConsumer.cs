using MassTransit;
using Datalake.PrivateApi;

namespace Datalake.Collector.Consumers;

public class SomethingHappenedConsumer(
	ILogger<SomethingHappenedConsumer> logger) : IConsumer<SomethingHappenedEvent>
{
	public async Task Consume(ConsumeContext<SomethingHappenedEvent> context)
	{
		var message = context.Message;

		logger.LogInformation("Received event: {message} on {timestamp}",
				message.Message, message.Timestamp);

		// Здесь ваша логика обработки
		await Task.Delay(1000); // Имитация обработки

		logger.LogInformation("Notification processed for event: {timestamp}", message.Timestamp);
	}
}
