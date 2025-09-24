using Datalake.PrivateApi;
using MassTransit;

namespace Datalake.Server.Services.Test;

/// <summary>
/// Тестируем отправку сообщений по RabbitMQ
/// </summary>
public class TestBackgroundService(
	IServiceScopeFactory serviceScopeFactory,
	ILogger<TestBackgroundService> logger) : BackgroundService
{
	/// <inheritdoc/>
	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));

		try
		{
			while (await timer.WaitForNextTickAsync(stoppingToken))
			{
				await Work(stoppingToken);
			}
		}
		catch (OperationCanceledException)
		{
			logger.LogInformation("{service} stopped by token", nameof(TestBackgroundService));
		}
		catch (Exception ex) when (ex is not OperationCanceledException)
		{
			logger.LogError(ex, nameof(TestBackgroundService));
			throw;
		}
	}

	private async Task Work(CancellationToken stoppingToken)
	{
		using var scope = serviceScopeFactory.CreateScope();
		var publishEndpoint = scope.ServiceProvider.GetRequiredService<IPublishEndpoint>();

		// Публикация события
		var message = new SomethingHappenedEvent
		{
			Message = "Hehe очередное событие",
			Timestamp = DateTime.UtcNow,
		};
		await publishEndpoint.Publish(message, stoppingToken);

		logger.LogInformation("Event sent: {timestamp}", message.Timestamp);
	}
}
