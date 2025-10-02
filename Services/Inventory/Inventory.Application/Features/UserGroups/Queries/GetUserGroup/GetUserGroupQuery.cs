using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.UserGroups;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroup;

public record GetUserGroupQuery : IQueryRequest<UserGroupInfo>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }

	public required Guid Guid { get; init; }
}
