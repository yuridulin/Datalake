using Datalake.Inventory.Application.Interfaces;
using Datalake.Inventory.Api.Models.UserGroups;
using Datalake.Shared.Application.Entities;

namespace Datalake.Inventory.Application.Features.UserGroups.Queries.GetUserGroupsTree;

public class GetUserGroupsTreeQuery : IQueryRequest<IEnumerable<UserGroupTreeInfo>>, IWithUserAccess
{
	public required UserAccessEntity User { get; init; }
}
