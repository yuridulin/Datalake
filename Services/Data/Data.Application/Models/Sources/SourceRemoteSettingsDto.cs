namespace Datalake.Data.Application.Models.Sources;

public record SourceRemoteSettingsDto
{
	public required string RemoteHost { get; init; }

	public required int RemotePort { get; init; }
}
