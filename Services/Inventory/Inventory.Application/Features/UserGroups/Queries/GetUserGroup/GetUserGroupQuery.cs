using Datalake.Contracts.Models.UserGroups;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroup;

public record GetUserGroupQuery : IQueryRequest<UserGroupInfo>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }

	public required Guid Guid { get; init; }
}
