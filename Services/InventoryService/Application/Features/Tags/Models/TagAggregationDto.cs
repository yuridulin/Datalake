using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Tags.Models;

public record TagAggregationDto
{
	public TagAggregation? Aggregation { get; init; }

	public AggregationPeriod? AggregationPeriod { get; init; }

	public int? SourceTagId { get; init; }

	public int? SourceTagBlockId { get; init; }
}
