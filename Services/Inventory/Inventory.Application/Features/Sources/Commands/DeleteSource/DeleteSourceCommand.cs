using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Commands.DeleteSource;

public record DeleteSourceCommand : ICommandRequest
{
	public required UserAccessValue User { get; init; }
	public required int SourceId { get; init; }
}
