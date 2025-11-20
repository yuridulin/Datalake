using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Sessions.Commands.CloseSession;

public record CloseSessionCommand : ICommandRequest
{
	public required string Token { get; init; }
}
