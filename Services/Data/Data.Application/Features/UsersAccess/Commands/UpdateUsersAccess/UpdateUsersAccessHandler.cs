using Datalake.Shared.Application.Interfaces;

namespace Datalake.Data.Application.Features.UsersAccess.Commands.UpdateUsersAccess;

public interface IUpdateUsersAccessHandler : ICommandHandler<UpdateUsersAccessCommand, bool> { }

public class UpdateUsersAccessHandler : IUpdateUsersAccessHandler
{
	public Task<bool> HandleAsync(UpdateUsersAccessCommand command, CancellationToken ct = default)
	{
		throw new NotImplementedException();
	}
}
