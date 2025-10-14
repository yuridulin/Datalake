using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Shared.Application.Entities;
using Datalake.Shared.Application.Interfaces;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroups;

public class GetUserGroupsQuery : IQueryRequest<IEnumerable<UserGroupInfo>>, IWithUserAccess
{
	public required UserAccessValue User { get; init; }
}
