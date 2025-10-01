using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.Users.Commands.DeleteUser;

public record DeleteUserCommand : ICommandRequest, IWithUserAccess
{
	public required	UserAccessEntity User { get; init; }

	public required Guid Guid { get; init; }
};
