namespace Datalake.Data.Application.Models.Tags;

public record TagScaleSettings
{
	public required float MinEu { get; init; }
	public required float MaxEu { get; init; }
	public required float MinRaw { get; init; }
	public required float MaxRaw { get; init; }

	public float GetScale() => (MaxEu - MinEu) / (MaxRaw - MinRaw);
}
