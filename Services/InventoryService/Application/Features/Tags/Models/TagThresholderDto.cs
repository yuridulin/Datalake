namespace Datalake.InventoryService.Application.Features.Tags.Models;

public record TagThresholderDto
{
	public int? ThresholdSourceTagId { get; init; }

	public int? ThresholdSourceTagBlockId { get; init; }

	public IEnumerable<TagThresholdDto>? Thresholds { get; init; }
}
