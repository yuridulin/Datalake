using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Sessions.Commands.OpenEnergoIdSession;

public record OpenEnergoIdSessionCommand : ICommandRequest
{
	public required Guid Guid { get; init; }
}
