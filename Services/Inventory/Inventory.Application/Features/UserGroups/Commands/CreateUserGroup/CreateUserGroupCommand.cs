using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Commands.CreateUserGroup;

public record CreateUserGroupCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public Guid? ParentGuid { get; init; }

	public string? Name { get; init; }

	public string? Description { get; init; }
}
