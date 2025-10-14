using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.UsersAccess.Commands.UpdateUsersAccess;

public record UpdateUsersAccessCommand : ICommandRequest
{
	public required IEnumerable<Guid> Guids { get; init; }
}
