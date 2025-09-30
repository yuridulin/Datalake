using Datalake.InventoryService.Application.Features.UserGroups.Models;
using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;

namespace Datalake.InventoryService.Application.Features.UserGroups.Commands.UpdateUserGroup;

public record UpdateUserGroupCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required Guid Guid { get; init; }

	public required string Name { get; init; }

	public string? Description { get; init; }

	public IEnumerable<UserRelationDto> Users { get; init; } = [];
}
