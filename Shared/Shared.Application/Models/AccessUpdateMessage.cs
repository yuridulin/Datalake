namespace Datalake.Shared.Application.Models;

public record AccessUpdateMessage
{
	public required DateTime Timestamp { get; init; }
	public required long Version { get; init; }
	public required IEnumerable<Guid> AffectedUsers { get; init; }
}
