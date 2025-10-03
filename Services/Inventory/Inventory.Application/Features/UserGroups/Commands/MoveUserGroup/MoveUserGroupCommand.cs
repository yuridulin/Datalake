using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.UserGroups.Commands.MoveUserGroup;

public record MoveUserGroupCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required Guid Guid { get; init; }

	public required Guid? ParentGuid { get; init; }
}
