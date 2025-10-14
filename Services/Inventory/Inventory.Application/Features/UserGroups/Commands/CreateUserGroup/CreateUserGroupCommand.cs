using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Commands.CreateUserGroup;

public record CreateUserGroupCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public Guid? ParentGuid { get; init; }

	public string? Name { get; init; }

	public string? Description { get; init; }
}
