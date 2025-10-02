using Datalake.Inventory.Application.Interfaces;
using Datalake.Shared.Domain.Entities;
using Datalake.Inventory.Api.Models.UserGroups;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroups;

public class GetUserGroupsQuery : IQueryRequest<IEnumerable<UserGroupInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
