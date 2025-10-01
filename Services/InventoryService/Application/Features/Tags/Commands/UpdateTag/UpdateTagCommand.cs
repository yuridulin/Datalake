using Datalake.InventoryService.Application.Features.Tags.Models;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Tags.Commands.UpdateTag;

public record UpdateTagCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required int Id { get; init; }

	public required string Name { get; init; }

	public string? Description { get; init; }

	public required TagType Type { get; init; }

	public TagResolution Resolution { get; init; } = TagResolution.NotSet;

	public int SourceId { get; init; } = (int)SourceType.Manual;

	public TagNumericDto? Numeric { get; init; }

	public TagInopcDto? Inopc { get; init; }

	public TagCalculationDto? Calculation { get; init; }

	public TagAggregationDto? Aggregation { get; init; }

	public TagThresholderDto? Thresholder { get; init; }
}
