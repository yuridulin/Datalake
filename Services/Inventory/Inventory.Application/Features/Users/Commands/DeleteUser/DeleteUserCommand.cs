using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand : ICommandRequest, IWithUserAccess
{
	public required	UserAccessEntity User { get; init; }

	public required Guid Guid { get; init; }
};
