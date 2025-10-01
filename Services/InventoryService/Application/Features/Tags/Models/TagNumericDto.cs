namespace Datalake.InventoryService.Application.Features.Tags.Models;

public record TagNumericDto
{
	public bool IsScaling { get; init; }

	public float MinEu { get; init; } = float.MinValue;

	public float MaxEu { get; init; } = float.MaxValue;

	public float MinRaw { get; init; } = float.MinValue;

	public float MaxRaw { get; init; } = float.MaxValue;
}
