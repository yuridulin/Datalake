using Datalake.Contracts.Public.Enums;

namespace Datalake.Data.Application.DataCollection.Models;

public record SourceSettingsDto
{
	public required int SourceId { get; init; }

	public required string SourceName { get; init; }

	public required SourceType SourceType { get; init; }

	public required bool IsDisabled { get; init; }

	public SourceRemoteSettingsDto? RemoteSettings { get; init; } = null;

	public required IEnumerable<TagSettingsDto> Tags { get; init; }
}
