using Datalake.Contracts.Models.UserGroups;
using Datalake.Domain.ValueObjects;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroups;

public class GetUserGroupsQuery : IQueryRequest<List<UserGroupInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }
}
