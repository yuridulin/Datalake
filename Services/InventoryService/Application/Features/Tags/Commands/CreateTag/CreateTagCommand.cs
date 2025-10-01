using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Enums;

namespace Datalake.InventoryService.Application.Features.Tags.Commands.CreateTag;

public record CreateTagCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required TagType Type { get; init; }

	public int? BlockId { get; init; }

	public int? SourceId { get; init; }

	public string? SourceItem { get; init; }
}
