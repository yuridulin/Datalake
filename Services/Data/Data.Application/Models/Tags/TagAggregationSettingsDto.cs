using Datalake.Domain.Enums;

namespace Datalake.Data.Application.Models.Tags;

public record TagAggregationSettingsDto
{
	public required int SourceTagId { get; init; }

	public required TagType SourceTagType { get; init; }

	public required TagResolution AggregatePeriod { get; init; }

	public required TagAggregation AggregateFunction { get; init; }
}
