using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroups;

public class GetUserGroupsQuery : IQueryRequest<IEnumerable<UserGroupInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
