using Datalake.Contracts.Public.Enums;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Tags.Commands.CreateTag;

public record CreateTagCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required TagType Type { get; init; }

	public int? BlockId { get; init; }

	public int? SourceId { get; init; }

	public string? SourceItem { get; init; }
}
