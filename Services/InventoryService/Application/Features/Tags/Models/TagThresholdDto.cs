namespace Datalake.InventoryService.Application.Features.Tags.Models;

public record TagThresholdDto
{
	public float InputValue { get; init; }

	public float OutputValue { get; init; }
}
