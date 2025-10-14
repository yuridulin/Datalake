using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroup;

public record GetUserGroupQuery : IQueryRequest<UserGroupInfo>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required Guid Guid { get; init; }
}
