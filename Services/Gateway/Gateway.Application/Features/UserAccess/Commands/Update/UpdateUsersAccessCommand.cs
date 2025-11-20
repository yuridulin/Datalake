using Datalake.Shared.Application.Interfaces;

namespace Datalake.Gateway.Application.Features.UserAccess.Commands.Update;

public record UpdateUsersAccessCommand : ICommandRequest
{
	public required bool IsAllUsers { get; init; }

	public required IEnumerable<Guid> Guids { get; init; }
}
