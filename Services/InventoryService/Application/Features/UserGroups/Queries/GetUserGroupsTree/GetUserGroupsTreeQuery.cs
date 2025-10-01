using Datalake.InventoryService.Application.Interfaces;
using Datalake.PrivateApi.Entities;
using Datalake.PublicApi.Models.UserGroups;

namespace Datalake.InventoryService.Application.Features.UserGroups.Queries.GetUserGroupsTree;

public class GetUserGroupsTreeQuery : IQueryRequest<IEnumerable<UserGroupTreeInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
