using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Commands.DeleteSource;

public record DeleteSourceCommand : ICommandRequest
{
	public required UserAccessEntity User { get; init; }
	public required int SourceId { get; init; }
}
