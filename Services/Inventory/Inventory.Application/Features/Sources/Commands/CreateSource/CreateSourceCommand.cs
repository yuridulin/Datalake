using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Sources.Commands.CreateSource;

public record CreateSourceCommand : ICommandRequest
{
	public required UserAccessValue User { get; init; }
}
