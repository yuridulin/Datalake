using Datalake.Contracts.Models.Data.Values;
using Datalake.Domain.Enums;

namespace Datalake.Data.Application.Models.Tags;

public record TagSettingsResponse
{
	public required int TagId { get; init; }

	public required Guid TagGuid { get; init; }

	public required TagType TagType { get; init; }

	public required string TagName { get; init; }

	public required TagResolution TagResolution { get; init; }

	public required int SourceId { get; init; }

	public required SourceType SourceType { get; init; }

	public required ValueResult Result { get; set; }
}
