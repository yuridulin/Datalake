using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.UserGroups.Commands.CreateUserGroup;

public record CreateUserGroupCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public Guid? ParentGuid { get; init; }

	public string? Name { get; init; }

	public string? Description { get; init; }
}
