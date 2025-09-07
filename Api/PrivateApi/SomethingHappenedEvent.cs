namespace Datalake.PrivateApi;

public record SomethingHappenedEvent
{
	public required string Message { get; init; }

	public DateTime Timestamp { get; init; }
}
