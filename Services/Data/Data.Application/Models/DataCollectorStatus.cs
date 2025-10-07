namespace Datalake.Data.Application.Models;

public record DataCollectorStatus
{
	public required string Name { get; init; }

	public bool IsRunning { get; init; }

	public int QueueSize { get; init; }

	public DateTime? LastActivity { get; init; }
}
