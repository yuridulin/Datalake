using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Commands.OpenEnergoIdSession;

public record OpenEnergoIdSessionCommand : ICommandRequest
{
	public required Guid Guid { get; init; }
}
