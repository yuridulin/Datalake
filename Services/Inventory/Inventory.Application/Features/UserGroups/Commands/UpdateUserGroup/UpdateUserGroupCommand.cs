using Datalake.Domain.ValueObjects;
using Datalake.Inventory.Application.Features.UserGroups.Models;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Commands.UpdateUserGroup;

public record UpdateUserGroupCommand : ICommandRequest, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required Guid Guid { get; init; }

	public required string Name { get; init; }

	public string? Description { get; init; }

	public IEnumerable<UserRelationDto> Users { get; init; } = [];
}
