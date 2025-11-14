using Datalake.Data.Application.Models.Tags;
using Datalake.Domain.Enums;

namespace Datalake.Data.Application.Models.Sources;

public record SourceSettingsDto
{
	public required int SourceId { get; init; }

	public required string SourceName { get; init; }

	public required SourceType SourceType { get; init; }

	public required bool IsDisabled { get; init; }

	public SourceRemoteSettingsDto? RemoteSettings { get; init; } = null;

	public required IEnumerable<TagSettingsDto> Tags { get; init; }

	public IEnumerable<TagSettingsDto> NotDeletedTags => Tags.Where(t => !t.IsDeleted);
}
