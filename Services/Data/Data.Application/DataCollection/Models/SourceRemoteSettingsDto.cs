namespace Datalake.Data.Application.DataCollection.Models;

public record SourceRemoteSettingsDto
{
	public required string RemoteHost { get; init; }

	public required int RemotePort { get; init; }
}
