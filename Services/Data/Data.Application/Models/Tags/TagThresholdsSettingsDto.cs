using Datalake.Domain.Enums;

namespace Datalake.Data.Application.Models.Tags;

public record TagThresholdsSettingsDto
{
	public required int SourceTagId { get; init; }

	public required TagType SourceTagType { get; init; }

	public required IEnumerable<TagThresholdDto> Thresholds { get; init; }

	public record TagThresholdDto
	{
		public required float InputValue { get; init; }

		public required float OutputValue { get; init; }
	}
}
