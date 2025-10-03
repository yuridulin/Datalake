using Datalake.Contracts.Public.Enums;
using Datalake.Inventory.Application.Features.Tags.Models;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Commands.UpdateTag;

public record UpdateTagCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required int Id { get; init; }

	public required string Name { get; init; }

	public string? Description { get; init; }

	public required TagType Type { get; init; }

	public TagResolution Resolution { get; init; } = TagResolution.None;

	public int SourceId { get; init; } = (int)SourceType.Manual;

	public bool? IsScaling { get; init; }

	public float? MinEu { get; init; }

	public float? MaxEu { get; init; }

	public float? MinRaw { get; init; }

	public float? MaxRaw { get; init; }

	public string? SourceItem { get; init; }

	public string? Formula { get; init; }

	public IEnumerable<TagInputDto> FormulaInputs { get; init; } = [];

	public TagAggregation? Aggregation { get; init; }

	public TagResolution? AggregationPeriod { get; init; }

	public int? SourceTagId { get; init; }

	public int? SourceTagBlockId { get; init; }

	public int? ThresholdSourceTagId { get; init; }

	public int? ThresholdSourceTagBlockId { get; init; }

	public IEnumerable<TagThresholdDto> Thresholds { get; init; } = [];
}
