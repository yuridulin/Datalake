using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.UserAccess.Commands.Update;

public record UpdateUsersAccessCommand : ICommandRequest
{
	public required IEnumerable<Guid> Guids { get; init; }
}
