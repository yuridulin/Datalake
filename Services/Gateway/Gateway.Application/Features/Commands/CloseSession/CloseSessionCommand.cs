using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.Commands.CloseSession;

public record CloseSessionCommand : ICommandRequest
{
	public required string Token { get; init; }
}
