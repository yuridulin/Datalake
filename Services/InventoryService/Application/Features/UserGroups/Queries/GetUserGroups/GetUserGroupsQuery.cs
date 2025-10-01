using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.UserGroups;

namespace Datalake.InventoryService.Application.Features.UserGroups.Queries.GetUserGroups;

public class GetUserGroupsQuery : IQueryRequest<IEnumerable<UserGroupInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
