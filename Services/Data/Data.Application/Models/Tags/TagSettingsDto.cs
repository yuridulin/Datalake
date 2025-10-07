using Datalake.Contracts.Public.Enums;

namespace Datalake.Data.Application.Models.Tags;

public record TagSettingsDto
{
	public required int TagId { get; init; }

	public required Guid TagGuid { get; init; }

	public required TagType TagType { get; init; }

	public required string TagName { get; init; }

	public required TagResolution TagResolution { get; init; }

	public required int SourceId { get; init; }

	public required SourceType SourceType { get; init; }

	public TagScaleSettings? ScaleSettings { get; init; } = null;

	public TagInopcSettingsDto? InopcSettings { get; init; } = null;

	public TagAggregationSettingsDto? AggregationSettings { get; init; } = null;

	public TagCalculationSettingsDto? CalculationSettings { get; init; } = null;

	public TagThresholdsSettingsDto? ThresholdsSettings { get; init; } = null;
}

public record TagScaleSettings
{
	public required float MinEu { get; init; }
	public required float MaxEu { get; init; }
	public required float MinRaw { get; init; }
	public required float MaxRaw { get; init; }

	public float GetScale() => (MaxEu - MinEu) / (MaxRaw - MinRaw);
}
