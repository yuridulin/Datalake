using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.UsersAccess.Commands.UpdateUsersAccess;

public record UpdateUsersAccessCommand : ICommandRequest
{
	public required IEnumerable<Guid> Guids { get; init; }
}
